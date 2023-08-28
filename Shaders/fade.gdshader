shader_type canvas_item;

uniform int   u_step;
uniform bool  u_mode;
uniform int   u_colour;

/* Functions */
	
// Subtract or add values based on u_colour
int SubA(int Val1, int Val2)
{
	if (u_colour == 1)
	{
		return Val1 + max(Val2 - u_step - Val1 + 252, 0);
	}
	else
	{
		return Val1 - max(Val2 - u_step + Val1, 0);
	}
}

// Subtract values based on u_step and Val2
int SubD(int Val1, int Val2)
{ 
    int Diff = max(756 - u_step - Val2, 0);
	if (u_colour == 1)
	{
		return Val1 + Diff;
	}
	else
	{
		return Val1 - Diff;
	}
}

// Add values based on u_step
int Flash(int Val1)
{
    return max(252 - u_step, Val1);
}

/* Main */

void fragment() {
	// Get the color value from the texture and multiply it by 255
	ivec4 Col = ivec4(texture(TEXTURE, UV) * 255.0);
	/*
	switch (u_colour)
	{
		case 2:
			if (u_step == 252) break;
			// If u_mode is true, apply the Flash function to the color
			if (u_mode)
			{
				Col.rgb = ivec3(Flash(Col.r), Flash(Col.g), Flash(Col.b));
			}
			
			// Otherwise, add a calculated value to the color and clamp it to a maximum of 252
			else
			{
				Col.rgb = min(Col.rgb + max(252 - u_step, 0), 252);
			}   
			break;
		case 1:
			break;
		default:
			break;
	}
	*/
	// If u_step is 0, set the color to black or white depending on u_colour
	if (u_step == 0)
	{
		Col.rgb = ivec3(u_colour == 0 ? 0 : 255);
	}
	
	// If u_colour is 2, alter the color based on u_mode and u_step
	else if (u_colour == 2) 
	{
		if (u_step != 252)
		{
			// If u_mode is true, apply the Flash function to the color
			if (u_mode)
			{
				Col.rgb = ivec3(Flash(Col.r), Flash(Col.g), Flash(Col.b));
			}
			
			// Otherwise, add a calculated value to the color and clamp it to a maximum of 252
			else
			{
				Col.rgb = min(Col.rgb + max(252 - u_step, 0), 252);
			}   
		}
	} 
	
	// If u_step is not 756 and u_colour is either 0 or 1, alter the color based on u_mode and the corresponding SubA or SubD function
	else if (u_step != 756)
	{
		if (u_colour == 1)
		{
			if (u_mode == true)
			{
				Col.rgb = ivec3(SubA(Col.r, 504 - Col.b - Col.g), SubA(Col.g, 252 - Col.b), SubA(Col.b, 0));
			}
			else
			{
				Col.rgb = ivec3(SubD(Col.r, 0), SubD(Col.g, 252 - Col.r), SubD(Col.b, 504 - Col.r - Col.g));
			}
		}
		else
		{
			if (u_mode == true)
			{
				Col.rgb = ivec3(SubA(Col.r, Col.b + Col.g), SubA(Col.g, Col.b), SubA(Col.b, 0));
			}
			else
			{
				Col.rgb = ivec3(SubD(Col.r, 0), SubD(Col.g, Col.r), SubD(Col.b, Col.r + Col.g));
			}
		}
	}
	
	// Set the final color value for the fragment and divide it by 255, then multiply it by the vertex color value
	COLOR = vec4(Col) / 255.0;
}
