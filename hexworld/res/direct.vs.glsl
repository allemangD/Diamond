#version 440

in vec3 v;

in vec3 pos;

uniform mat4 proj;
uniform mat4 view;

void main ()
{
	gl_Position = proj * view * vec4(pos + v, 1);
}