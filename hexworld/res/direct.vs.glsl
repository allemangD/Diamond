#version 440

in vec3 v;

void main ()
{
	gl_Position = vec4(v, 1);
}