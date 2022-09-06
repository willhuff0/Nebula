##VERTEX
#version 410 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

void main() {
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
    TexCoords = aTexCoords;
}

##FRAGMENT
#version 410 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D depthMap;

void main() {
    float depthValue = texture(depthMap, TexCoords).r;
    FragColor = vec4(vec3(depthValue), 1.0);
}