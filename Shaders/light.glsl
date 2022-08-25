##VERTEX
#version 410 core

layout(location = 0) in vec3 a_pos;
layout(location = 1) in vec3 a_normal;
layout(location = 2) in vec3 a_uv;

uniform mat4 matrix_transform;
uniform mat4 matrix_viewProjection;

void main(void) {
    gl_Position = vec4(a_pos, 1.0) * matrix_transform * matrix_viewProjection;
}


##FRAGMENT
#version 410 core

out vec4 FragColor;

void main() {
    FragColor = vec4(1.0);
}