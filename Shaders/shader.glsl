##VERTEX
#version 410 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

void main(void) {
    texCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0);
}


##FRAGMENT
#version 410 core

out vec4 outputColor;

in vec texCoord;

uniform sampler2D texture0;

void main() {
    outputColor = vec4(mix(vertexColor, globalColor, 0.5), 1.0);
}