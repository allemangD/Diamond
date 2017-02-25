#version 440

in vec2 vcoord;
in float light;

uniform sampler2D tex;

void main ()
{
	vec4 color = light * texture(tex, vcoord);
	gl_FragColor = clamp(color, 0, 1);
}