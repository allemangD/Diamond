#version 440

in vec3 locpos;
in vec3 glbpos;
in vec2 coord;
in vec3 norm;

out vec2 vcoord;
out float light;

uniform mat4 view;
uniform mat4 proj;

void main ()
{
	mat4 pv = proj * view;
	vec3 position = glbpos + locpos;
	gl_Position = pv * vec4(position, 1);
	vcoord = coord;
	
	light = dot(vec3(0, 0, 1), norm) / 2 + .5;
}