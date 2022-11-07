/* 
ATTENTION:
	# Keep code comaptible to GLSL version 1.2 (the lowest), since many Laptops, even though they
	  are new (including mine 2010), may not support higher versions.
*/

varying vec4 shared_Color;

void main(void)
{
    gl_FragColor = shared_Color;
}