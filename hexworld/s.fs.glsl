#version 440

in vec2 vcoord;

uniform sampler2D tex;

void main ()
{
	gl_FragColor = texture(tex, vcoord);
}