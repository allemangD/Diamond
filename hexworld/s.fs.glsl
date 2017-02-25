#version 440

in vec2 vcoord;
in float light;

uniform sampler2D tex;

void main ()
{
	vec4 color = texture(tex, vcoord);
	color.xyz *= light;
	gl_FragColor = clamp(color, 0, 1);
}