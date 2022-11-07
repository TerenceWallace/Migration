/* 
ATTENTION:
	# Keep code comaptible to GLSL version 1.2 (the lowest), since many Laptops, even though they
	  are new (including mine 2010), may not support higher versions.
*/
uniform sampler2D tex_Stage1;


uniform mat4 worldMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat4 worldITMatrix;
uniform float timeMillis;

varying vec4 shared_Color;
varying vec3 shared_Normal;
varying vec3 shared_Position;
varying vec2 shared_TexCoord1;

//{PARAMETERIZATION}

void main(void)
{
	gl_Position = worldMatrix * viewMatrix * modelMatrix * vec4(gl_Vertex.xy, gl_Vertex.z * HEIGHTSCALE, 1.0);
	shared_Color = gl_Color;
	shared_Position = gl_Vertex.xyz;
	shared_Normal = gl_Normal.xyz;
	shared_TexCoord1 = gl_Vertex.xy / 32.0;
}