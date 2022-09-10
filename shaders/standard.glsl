##VERTEX
#version 310 es

layout (location = 0) in vec3 a_pos;
layout (location = 1) in vec3 a_normal;
layout (location = 2) in vec2 a_texCoord;

out vec2 v_texCoord;
out vec3 v_worldPos;
out vec3 v_normal;

uniform int nebula_shadowCasterCount;
out vec4 v_shadowCasterCoords[10];
uniform mat4 nebula_shadowMatrices[10];

uniform mat4 nebula_matrix_transform;
uniform mat4 nebula_matrix_viewProjection;

void main() {
    v_texCoord = a_texCoord;
    v_worldPos = vec3(nebula_matrix_transform * vec4(a_pos, 1.0));
    v_normal = mat3(nebula_matrix_transform) * a_normal;
    
    for(int i = 0; i < nebula_shadowCasterCount; ++i) {
        v_shadowCasterCoords[i] = vec4(a_pos, 1.0) * nebula_matrix_transform * nebula_shadowMatrices[i];
    }

    //gl_Position = vec4(v_worldPos, 1.0) * matrix_view * matrix_projection;
    gl_Position = vec4(a_pos, 1.0) * nebula_matrix_transform * nebula_matrix_viewProjection;
}


##FRAGMENT
precision highp float;
#version 310 es

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

uniform int nebula_shadowCasterCount;
uniform sampler2D nebula_shadowMaps[10];
in vec4 v_shadowCasterCoords[10];

uniform Material nebula_material;

uniform int nebula_directionalLightCount;
uniform DirectionalLight nebula_directionalLights[2];
uniform int nebula_pointLightCount;
uniform PointLight nebula_pointLights[8];

uniform vec3 nebula_viewPos;

const float PI = 3.14159265359;

vec3 getNormalFromMap() {
    vec3 tangentNormal = texture(nebula_material.texture_normal, v_texCoord).xyz * 2.0 - 1.0;

    vec3 Q1 = dFdx(v_worldPos);
    vec3 Q2 = dFdy(v_worldPos);
    vec2 st1 = dFdx(v_texCoord);
    vec2 st2 = dFdy(v_texCoord);

    vec3 N = normalize(v_normal);
    vec3 T = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B = -normalize(cross(N, T));
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
    vec3 albedo = pow(texture(nebula_material.texture_albedo, v_texCoord).rgb, vec3(2.2));
    float metallic = texture(nebula_material.texture_metallic, v_texCoord).r;
    float roughness = texture(nebula_material.texture_roughness, v_texCoord).r;
    float ao = texture(nebula_material.texture_ao, v_texCoord).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(nebula_viewPos - v_worldPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);

    // Directional Lights
    for (int i = 0; i < nebula_directionalLightCount; ++i) {
        DirectionalLight light = nebula_directionalLights[i];

        vec3 _L = light.direction;

        Lo += processLight(albedo, metallic, roughness, F0, _L, 1.0, V, N, light.color, light.intensity);
    }

    // Point Lights
    for (int i = 0; i < nebula_pointLightCount; ++i) {
        PointLight light = nebula_pointLights[i];

        vec3 _L = light.position - v_worldPos;

        float distance = length(_L) / 4;
        float attenuation = 1.0 / (distance * distance);

        Lo += processLight(albedo, metallic, roughness, F0, _L, attenuation, V, N, light.color, light.intensity);
    }

    float totalShadow = 0.0;

    // Shadow Casters
    for (int i = 0; i < nebula_shadowCasterCount; ++i) {
        vec4 shadowCoord = v_shadowCasterCoords[i];
        vec3 projCoords = shadowCoord.xyz / shadowCoord.w;
        projCoords = projCoords * 0.5 + 0.5;

        if (projCoords.z > 1.0) continue;

        float closestDepth = texture(nebula_shadowMaps[i], projCoords.xy).r;
        float currentDepth = projCoords.z;

        float shadow = 0.0;
        vec2 texelSize = 1.0 / textureSize(nebula_shadowMaps[i], 0);
        for(int x = -1; x <= 1; ++x)
        {
            for(int y = -1; y <= 1; ++y)
            {
                float pcfDepth = texture(nebula_shadowMaps[i], projCoords.xy + vec2(x, y) * texelSize).r; 
                shadow += currentDepth > pcfDepth ? 0.4 : 0.0;
            }    
        }
        shadow /= 9.0;

        totalShadow += shadow;
    }

    vec3 ambient = vec3(0); //vec3(0.03) * albedo;
    vec3 color = (ambient + Lo * (1 - totalShadow)) * ao;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));

    FragColor = vec4(color, 1.0);
}
