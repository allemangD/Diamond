#version 440

in vec2 vTexCoord;

uniform sampler2D tex;

void main ()
{
	gl_FragColor = texture(tex, vTexCoord) + vec4(.1);
}