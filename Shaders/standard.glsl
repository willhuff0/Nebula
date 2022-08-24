##VERTEX
#version 410 core

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec2 a_texCoord;

out vec2 v_texCoord;
out vec3 v_worldPos;
out vec3 v_normal;

uniform mat4 matrix_transform;
uniform mat4 matrix_viewProjection;

void main() {
    v_texCoord = a_texCoord;
    v_worldPos = vec3(matrix_transform * vec4(a_pos, 1.0));
    v_normal = mat3(matrix_transform) * a_normal;

    gl_Position = vec4(a_pos, 1.0) * matrix_transform * matrix_viewProjection;
}


##FRAGMENT
#version 410 core

struct Material {
    sampler2D texture_albedo;
    sampler2D texture_normal;
    sampler2D texture_metallic;
    sampler2D texture_roughness;
    sampler2D texture_ao;
};

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
};

out vec4 FragColor;
in vec2 v_texCoord;
in vec3 v_worldPos;
in vec3 v_normal;

uniform Material material;

uniform int directionalLightCount;
uniform DirectionalLight directionalLights[2];
uniform int pointLightCount;
uniform PointLight pointLights[8];

uniform vec3 viewPos;

const float PI = 3.14159265359;

vec3 getNormalFromMap() {
    vec3 tangentNormal = texture(material.texture_normal, v_texCoord).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(v_worldPos);
    vec3 Q2  = dFdy(v_worldPos);
    vec2 st1 = dFdx(v_texCoord);
    vec2 st2 = dFdy(v_texCoord);

    vec3 N   = normalize(v_normal);
    vec3 T  = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B  = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}

float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}
vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

vec3 processLight(vec3 albedo, float metallic, float roughness, vec3 F0, vec3 _L, float attenuation, vec3 V, vec3 N, vec3 color, float intensity) {
    vec3 L = normalize(_L);
    vec3 H = normalize(V + L);
    
    vec3 radiance = color * intensity * attenuation;

    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0)  + 0.0001;
    vec3 specular = numerator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    
    kD *= 1.0 - metallic;

    float NdotL = max(dot(N, L), 0.0);
    return (kD * albedo / PI + specular) * radiance * NdotL;
}

void main() {
    vec3 albedo = pow(texture(material.texture_albedo, v_texCoord).rgb, vec3(2.2));
    float metallic = texture(material.texture_metallic, v_texCoord).r;
    float roughness = texture(material.texture_roughness, v_texCoord).r;
    float ao = texture(material.texture_ao, v_texCoord).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(viewPos - v_worldPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);

    // Directional Lights
    for (int i = 0; i < directionalLightCount; ++i) {
        DirectionalLight light = directionalLights[i];

        vec3 _L = light.direction;

        Lo += processLight(albedo, metallic, roughness, F0, _L, 1.0, V, N, light.color, light.intensity);
    }

    // Point Lights
    for (int i = 0; i < pointLightCount; ++i) {
        PointLight light = pointLights[i];

        vec3 _L = light.position - v_worldPos;

        float distance = length(_L) / 4;
        float attenuation = 1.0 / (distance * distance);

        Lo += processLight(albedo, metallic, roughness, F0, _L, attenuation, V, N, light.color, light.intensity);
    }

    vec3 ambient = vec3(0); //vec3(0.03) * albedo;
    vec3 color = (ambient + Lo) * ao;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1.0);
}
