#version 440

in vec3 position;
in vec3 glbpos;
in vec2 uv;
in vec3 normal;

out vec2 vcoord;
out float light;

uniform mat4 view;
uniform mat4 proj;

void main ()
{
	mat4 pv = proj * view;
	vec3 pos = glbpos + position;
	gl_Position = pv * vec4(pos, 1);
	vcoord = uv;
	
	light = dot(vec3(0, 0, 1), normal) / 2 + .5;
}