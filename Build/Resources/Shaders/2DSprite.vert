/* 
ATTENTION:
	# Keep code comaptible to GLSL version 1.2 (the lowest), since many Laptops, even though they
	  are new (including mine), may not support higher versions.
*/

uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

varying vec4 shared_Color;
varying vec2 shared_TexCoord1;

void main(void)
{
/*
	The following is a little trick to use apply Z filtering for isometric projections,
	without having the Z value affecting the objects position. This is important and done
	via backing up the Z value, projecting the vertex with a zero Z value using isometric
	projection, and finally resetting the original Z value to the final position.
*/
	 vec4 pos = gl_Vertex;
	 float zBackup = pos.z;
	 pos = vec4(pos.xy, 0, pos.w);
     gl_Position = worldMatrix * viewMatrix * modelMatrix * pos;
	 gl_Position = vec4(gl_Position.xy, zBackup, gl_Position.w);
	 shared_Color = gl_Color;
	 shared_TexCoord1 = vec2(gl_MultiTexCoord0);
}