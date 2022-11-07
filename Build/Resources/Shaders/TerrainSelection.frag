 /*
 * ATTENTION:
 *		# Keep code comaptible to GLSL version 1.2
 */
uniform float SelectionOffsetXID;
uniform float SelectionOffsetYID;

varying vec4 shared_Color;
varying vec3 shared_Position;

void main(void)
{
    gl_FragColor = vec4(floor(shared_Position.x - SelectionOffsetXID) / 255.0, floor(shared_Position.y - SelectionOffsetYID) / 255.0, 0.0, 1.0);
}