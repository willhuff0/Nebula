##VERTEX
#version 410 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection; // im a g

out vec3 Normal;
out vec3 FragPos;

void main(void) {
    gl_Position = vec4(aPos, 1.0) * model * view * projection;
    FragPos = vec3(vec4(aPos, 1) * model);
    Normal = aNormal * mat3(transpose(inverse(model)));
}


##FRAGMENT
#version 410 core

out vec4 FragColor;

void main() {
    FragColor = vec4(1.0);
}