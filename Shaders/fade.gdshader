shader_type canvas_item;

const int LIMIT1 = 252;
const int LIMIT2 = 504;
const int LIMIT3 = 756;

uniform int  u_step;
uniform bool u_mode;
uniform int  u_colour;

/* Functions */
	
// Subtract or add values based on u_colour
ivec3 SubA(ivec3 Val1, ivec3 Val2)
{
	if (u_colour == 1)
	{
		return Val1 + max(Val2 - u_step - Val1 + LIMIT1, 0);
	}
	else
	{
		return Val1 - max(Val2 - u_step + Val1, 0);
	}
}

// Subtract values based on u_step and Color
ivec3 SubD(ivec3 Color)
{ 
	return max(LIMIT3 - u_step - Color, 0) * (u_colour == 1 ? 1 : -1);
}

/* Main */

void fragment() {
	// Get the color value from the texture and multiply it by 255
	ivec3 Color = ivec3(texture(TEXTURE, UV).rgb * 255.0);
	
	// If u_step is 0, set the color to black or white depending on u_colour
	if (u_step == 0)
	{
		Color = ivec3(u_colour == 0 ? 0 : 255);
	}
	else switch (u_colour)
	{
		// If u_colour is 2, alter the color based on u_mode and u_step
		case 2:
			if (u_step == LIMIT1) break;
			// If u_mode is true, apply the Flash function to the color
			if (u_mode)
			{
				Color = max(Color, LIMIT1 - u_step);
			}
			
			// Otherwise, add a calculated value to the color 
			// and clamp it to a maximum of LIMIT1
			else
			{
				Color = min(Color + max(LIMIT1 - u_step, 0), LIMIT1);
			}   
			break;
		// If u_step is not LIMIT3 and u_colour is 1, alter the color 
		// based on u_mode and the corresponding SubA or SubD function
		case 1:
			if (u_step == LIMIT3) break;
			if (u_mode)
			{
				Color = SubA(Color, ivec3(LIMIT2 - Color.b - Color.g, LIMIT1 - Color.b, 0));
			}
			else
			{
				Color += SubD(ivec3(0, LIMIT1 - Color.r, LIMIT2 - Color.r - Color.g));
			}
			break;
		// If u_step is not LIMIT3 and u_colour is 0, alter the color 
		// based on u_mode and the corresponding SubA or SubD function
		default:
			if (u_step == LIMIT3) break;
			if (u_mode)
			{
				Color = SubA(Color, ivec3(Color.b + Color.g, Color.b, 0));
			}
			else
			{
				Color += SubD(ivec3(0, Color.r, Color.r + Color.g));
			}
			break;
	}
	
	// Set the final color value for the fragment and divide it by 255
	COLOR.rgb = vec3(Color) / 255.0;
}
