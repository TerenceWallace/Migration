 /*
 * ATTENTION:
 *		# Keep code comaptible to GLSL version 1.2
 */
uniform sampler2D tex_Stage1;
uniform sampler2D tex_Stage2;
uniform sampler2D tex_Stage3;
uniform sampler2D tex_Stage4;
uniform sampler2D tex_Stage5;
uniform sampler2D tex_Stage6;

vec3 LightPos = vec3(-0.7, 0.0, 0.7);
vec3 LightColor = vec3(0.7, 0.7, 0.7);

varying vec4 shared_Color;
varying vec3 shared_Normal;
varying vec3 shared_Position;


#define ONE 0.00390625
#define ONEHALF 0.001953125

/*
 * 2D simplex noise. Somewhat slower but much better looking than classic noise.
 */
float snoise(const in vec2 P) 
{
// Skew and unskew factors are a bit hairy for 2D, so define them as constants
// This is (sqrt(3.0)-1.0)/2.0
#define F2 0.366025403784
// This is (3.0-sqrt(3.0))/6.0
#define G2 0.211324865405

  // Skew the (x,y) space to determine which cell of 2 simplices we're in
 	float s = (P.x + P.y) * F2;   // Hairy factor for 2D skewing
  vec2 Pi = floor(P + s);
  float t = (Pi.x + Pi.y) * G2; // Hairy factor for unskewing
  vec2 P0 = Pi - t; // Unskew the cell origin back to (x,y) space
  Pi = Pi * ONE + ONEHALF; // Integer part, scaled and offset for texture lookup

  vec2 Pf0 = P - P0;  // The x,y distances from the cell origin

  // For the 2D case, the simplex shape is an equilateral triangle.
  // Find out whether we are above or below the x=y diagonal to
  // determine which of the two triangles we're in.
  vec2 o1;
  if(Pf0.x > Pf0.y) o1 = vec2(1.0, 0.0);  // +x, +y traversal order
  else o1 = vec2(0.0, 1.0);               // +y, +x traversal order

  // Noise contribution from simplex origin
  vec2 grad0 = texture2D(tex_Stage1, Pi).rg * 4.0 - 1.0;
  float t0 = 0.5 - dot(Pf0, Pf0);
  float n0;
  if (t0 < 0.0) n0 = 0.0;
  else {
    t0 *= t0;
    n0 = t0 * t0 * dot(grad0, Pf0);
  }

  // Noise contribution from middle corner
  vec2 Pf1 = Pf0 - o1 + G2;
  vec2 grad1 = texture2D(tex_Stage1, Pi + o1*ONE).rg * 4.0 - 1.0;
  float t1 = 0.5 - dot(Pf1, Pf1);
  float n1;
  if (t1 < 0.0) n1 = 0.0;
  else {
    t1 *= t1;
    n1 = t1 * t1 * dot(grad1, Pf1);
  }
  
  // Noise contribution from last corner
  vec2 Pf2 = Pf0 - vec2(1.0-2.0*G2);
  vec2 grad2 = texture2D(tex_Stage1, Pi + vec2(ONE, ONE)).rg * 4.0 - 1.0;
  float t2 = 0.5 - dot(Pf2, Pf2);
  float n2;
  if(t2 < 0.0) n2 = 0.0;
  else {
    t2 *= t2;
    n2 = t2 * t2 * dot(grad2, Pf2);
  }

  // Sum up and scale the result to cover the range [-1,1]
  return 70.0 * (n0 + n1 + n2);
}

//{PARAMETERIZATION}

#define BORDERWIDTH_00 0.1
#define BORDERWIDTH_01 0.1
#define BORDERWIDTH_02 0.1
#define BORDERWIDTH_03 0.1

#define MARGIN_00 -0.66
#define MARGIN_01 -0.33
#define MARGIN_02 0.0
#define MARGIN_03 0.33

void main(void)
{
	// calculate normals
	vec3 v1 =  vec3(shared_Position.xy, texture2D(tex_Stage6, shared_Position.xy / MAPSIZE).r * HEIGHTSCALE);
	vec3 v2 =  vec3(vec2(shared_Position.x + 1.0, shared_Position.y), texture2D(tex_Stage6, vec2(shared_Position.x + 1.0, shared_Position.y) / MAPSIZE).r * HEIGHTSCALE);
	vec3 v3 =  vec3(vec2(shared_Position.x, shared_Position.y + 1.0), texture2D(tex_Stage6, vec2(shared_Position.x, shared_Position.y + 1.0) / MAPSIZE).r * HEIGHTSCALE);

	v1 = vec3(v1.xy, v1.z * NORMALSCALE);
	v2 = vec3(v2.xy, v2.z * NORMALSCALE);
	v3 = vec3(v3.xy, v3.z * NORMALSCALE);

	vec3 N = normalize(cross(v3-v2, v1-v2));
	float l = dot(N, normalize(LightPos));

	// compute noise
	float noise = snoise(shared_Position.xy * 5.0) / (HEIGHTSCALE);
	float redNoise = snoise((shared_Position.xy + 10.0) / RED_FREQ);
	float greenNoise = snoise((shared_Position.xy + 40.0) / GREEN_FREQ);
	float blueNoise = snoise(shared_Position.xy / BLUE_FREQ);
	float ground = shared_Normal.y + noise;

	// smoother layer transisition; important when blending textures with huge differences in lumination, Rock/Sand for example
	float level1_step = smoothstep(MARGIN_00 - BORDERWIDTH_00, MARGIN_00 + BORDERWIDTH_00, ground);
	float level2_step = smoothstep(MARGIN_01 - BORDERWIDTH_01, MARGIN_01 + BORDERWIDTH_01, ground);
	float level3_step = smoothstep(MARGIN_02 - BORDERWIDTH_02, MARGIN_02 + BORDERWIDTH_02, ground);
	float level4_step = smoothstep(MARGIN_03 - BORDERWIDTH_03, MARGIN_03 + BORDERWIDTH_03, ground);

	vec3 l0mat = texture2D(tex_Stage1, shared_Position.xy / TEXSCALE_00).rgb;
	vec3 l1mat = texture2D(tex_Stage2, shared_Position.xy / TEXSCALE_01).rgb;
	vec3 l2mat = texture2D(tex_Stage3, shared_Position.xy / TEXSCALE_02).rgb; 
	vec3 l3mat = texture2D(tex_Stage4, shared_Position.xy / TEXSCALE_03).rgb;
	vec3 l4mat = texture2D(tex_Stage5, shared_Position.xy / TEXSCALE_04).rgb;

	l0mat = vec3(l0mat.b + blueNoise / BLUENOISESCALE_00, l0mat.g + greenNoise / GREENNOISESCALE_00, l0mat.r + redNoise / REDNOISESCALE_00);
	l1mat = vec3(l1mat.b + blueNoise / BLUENOISESCALE_01, l1mat.g + greenNoise / GREENNOISESCALE_01, l1mat.r + redNoise / REDNOISESCALE_01);
	l2mat = vec3(l2mat.b + blueNoise / BLUENOISESCALE_02, l2mat.g + greenNoise / GREENNOISESCALE_02, l2mat.r + redNoise / REDNOISESCALE_02);
	l3mat = vec3(l3mat.b + blueNoise / BLUENOISESCALE_03, l3mat.g + greenNoise / GREENNOISESCALE_03, l3mat.r + redNoise / REDNOISESCALE_03);
	l4mat = vec3(l4mat.b + blueNoise / BLUENOISESCALE_04, l4mat.g + greenNoise / GREENNOISESCALE_04, l4mat.r + redNoise / REDNOISESCALE_04);

	vec3 material = (
		(1.0 - level1_step) * l0mat + 
		level1_step * (1.0 - level2_step) * l1mat + 
		level2_step * (1.0 - level3_step) * l2mat + 
		level3_step * (1.0 - level4_step) * l3mat + 
		level4_step * l4mat);

	//gl_FragColor = vec4((0.2 + l * LightColor) * vec3(1.0,1.0,1.0), 1.0);
	gl_FragColor = vec4((0.2 + l * LightColor) * (material * (1.0 - shared_Normal.z) + shared_Normal.z * l0mat), 1.0);
}