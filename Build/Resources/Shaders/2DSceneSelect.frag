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
	
	/*
		If pixel is not fully translucent, we make it fully opaque,
		because otherwise the color won't match correctly in later
		pixel selection. Further instead of using the texture color,
		we are using the ones provided by selection model.
	*/
	gl_FragColor = vec4(shared_Color.x, shared_Color.y, shared_Color.z, ceil(color.w));
}