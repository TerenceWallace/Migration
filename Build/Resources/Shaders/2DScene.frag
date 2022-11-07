/* 
ATTENTION:
	# Keep code comaptible to GLSL version 1.2 (the lowest), since many Laptops, even though they
	  are new (including mine), may not support higher versions.
*/
uniform sampler2D tex_Stage1;

varying vec4 shared_Color;
varying vec2 shared_TexCoord1;

void main(void)
{
	vec4 color = texture2D(tex_Stage1, shared_TexCoord1);
	
	gl_FragColor = vec4(color.z,color.y,color.x,color.w * shared_Color.w);
}