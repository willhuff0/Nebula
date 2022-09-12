##VERTEX
#version 310 es

layout (location = 0) in vec3 a_pos;

uniform mat4 nebula_matrix_transform;
uniform mat4 nebula_matrix_viewProjection;

void main() {
    gl_Position = vec4(a_pos, 1.0) * nebula_matrix_transform * nebula_matrix_viewProjection;
}

##FRAGMENT
#version 310 es

void main() {
    
}