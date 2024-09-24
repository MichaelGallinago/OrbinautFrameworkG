shader_type canvas_item;

const int LIMIT1 = 252;
const int LIMIT2 = 504;
const int LIMIT3 = 756;
const float CHANNEL_LIMIT = 255.0;

const vec3 ONE  = vec3(1.0);
const vec3 ZERO = vec3(0.0);

uniform int u_step;
uniform int u_type;

ivec3 getColour(ivec3 colour)
{
	switch (u_type)
	{
		case 0: return min(colour - LIMIT3 + u_step + ivec3(0, colour.r, colour.r + colour.g), colour);
		case 1: return min(u_step - ivec3(colour.b + colour.g, colour.b, 0), colour);
		case 2: return min(colour, u_step);
		case 3: return max(colour - max(LIMIT1 - u_step, 0), 0);
		case 4: return max(ivec3(LIMIT2 - colour.b - colour.g, LIMIT1 - colour.b, 0) - u_step + LIMIT1, colour);
		case 5: return max(colour + LIMIT3 - u_step - ivec3(0, LIMIT1 - colour.r, LIMIT2 - colour.r - colour.g), colour);
		case 6: return max(colour, LIMIT1 - u_step);
		case 7: return min(colour + max(LIMIT1 - u_step, 0), LIMIT1);
		default: return colour;
	}
}

void fragment() {
	// Get the color value from the texture and multiply it by 255
	ivec3 Colour = ivec3(texture(TEXTURE, UV).rgb * CHANNEL_LIMIT);

	// Set the final color value for the fragment and divide it by 255
	COLOR.rgb = u_step > 0 ?
		vec3(getColour(Colour)) / CHANNEL_LIMIT :
		(u_type < 4 ? ZERO : ONE);
}
