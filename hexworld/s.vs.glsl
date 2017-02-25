#version 440

in vec3 pos;
in vec2 coord;

out vec2 vcoord;

uniform mat4 view;
uniform mat4 proj;

void main ()
{
	gl_Position = proj*view*vec4(pos, 1);
	vcoord = coord;
}