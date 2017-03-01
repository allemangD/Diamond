#version 440

// Wavefront vertices
in vec3 v;
in vec2 vt;
in vec3 vn;

// Instance data
in vec3 global_pos;

uniform mat4 view;
uniform mat4 proj;

out vec2 vcoord;
out float light;

void main ()
{
	mat4 proj_view = proj * view;
	vec3 pos = global_pos + v;

	gl_Position = proj_view * vec4(pos, 1);

	vcoord = vt;
	
	light = dot(vec3(.2, .3, 1), vn) / 2 + .5;
}