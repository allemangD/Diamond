#version 440

layout (location = 0) in vec3 vPosition;
layout (location = 1) in vec3 vColor;
layout (location = 2) in vec3 vOffset;

out vec4 color;

layout (location = 0) uniform mat4 view;
layout (location = 1) uniform mat4 projection;

void main()
{
	vec3 pos = vPosition + vOffset;
    gl_Position = projection * view * vec4(pos, 1.0);
    color = vec4( vColor, 1.0);
}