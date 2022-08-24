##VERTEX
#version 410 core

layout (location = 0) in vec3 a_pos;

uniform mat4 matrix_transform;
uniform mat4 matrix_viewProjection;

void main() {
    gl_Position = vec4(a_pos, 1.0) * matrix_transform * matrix_viewProjection;
}

##FRAGMENT
#version 410 core

void main() {

}