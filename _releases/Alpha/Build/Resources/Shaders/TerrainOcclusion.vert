/* 
ATTENTION:
	# Keep code comaptible to GLSL version 1.2 (the lowest), since many Laptops, even though they
	  are new (including mine 2010), may not support higher versions.
*/
uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

varying vec4 shared_Color;

void main(void)
{
	gl_Position = worldMatrix * viewMatrix * modelMatrix * gl_Vertex;
	shared_Color = gl_Color;
}