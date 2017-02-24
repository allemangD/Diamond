#version 440

in vec3 pos;
in vec2 texCoord;

out vec2 vTexCoord;

layout (location=1) uniform mat4 view;
layout (location=2) uniform mat4 proj;

void main ()
{
	gl_Position = proj*view*vec4(pos, 1);
	vTexCoord = texCoord;
}