/*
 *
 * ATTENTION:
 *		# Keep code comaptible to GLSL version 1.2
 */
uniform sampler2D tex_Stage1;
uniform sampler2D tex_Stage7;
uniform float timeMillis;

vec3 LightPos = vec3(-0.7, 0.0, 0.7);
vec3 LightColor = vec3(0.7, 0.7, 0.7);

varying vec4 shared_Color;
varying vec3 shared_Normal;
varying vec3 shared_Position;
varying vec2 shared_TexCoord1;

//{PARAMETERIZATION}

void main(void)
{
	// calculate normals
	vec3 N = vec3(0.0,0.0,1.0);
	float l = dot(N, normalize(LightPos));
	vec3 water = (texture2D(tex_Stage7, (shared_Position.xy + timeMillis / 500.0) / 32.0) / 2.0).bgr;
	float alpha = smoothstep(WATERHEIGHT - 0.3, WATERHEIGHT + 0.2, shared_Position.z);


	vec4 color = vec4(
		(0.1 + l * LightColor) * water,
		0.9 * (1.0 -alpha));
		
    gl_FragColor = color;
}