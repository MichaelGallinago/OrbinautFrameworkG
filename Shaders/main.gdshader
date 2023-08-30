shader_type canvas_item;

// Also used in c_engine -> Create Event -> Initialise Palette
#define PaletteLimit 256

#define Alpha vec4(1.0, 0.0, 1.0, 1.0)

#define v_vColour vec4(1.0, 1.0, 1.0, 1.0)

uniform float u_bound;

uniform sampler2D u_texture1;
uniform vec3	  u_UVs1;
uniform vec2	  u_texelSize1;
uniform float     u_index1[PaletteLimit];

uniform sampler2D u_texture2;
uniform vec3      u_UVs2;
uniform vec2      u_texelSize2;
uniform float     u_index2[PaletteLimit];

uniform bool  u_parallaxActive;
uniform vec2  u_offset;
uniform float u_width;
uniform float u_scaling;
uniform float u_inclineHeight;
uniform float u_inclineStep;
uniform float u_height;
	
uniform bool u_overlapActive;
uniform vec2 u_mapSizeSurf;
uniform sampler2D u_surface;

// Background Parallax
vec2 parallax(vec2 uv, vec2 texturePixelSize) 
{		
	vec2 Position = uv / texturePixelSize;
	// Set the pixel offset (can be negative)
	float OffsetX = u_offset.x; 
	
	// Adjust the pixel offset based on height and incline step
	if (u_inclineHeight != 0.0)
	{
		OffsetX *= floor((Position.y / u_scaling - u_height) / u_inclineHeight + 1.0) * u_inclineStep + 1.0;
	}
	
	// Normalise the offset value to ensure the final pixel position is within u_width
	OffsetX = mod(mod(floor(OffsetX) + Position.x - u_offset.y, u_width) + u_width, u_width) - Position.x;

	// Return the new pixel position with the offset converted to uv coordinates
    return vec2(uv.x + OffsetX * texturePixelSize.x, uv.y);
}

// Surface Stack
vec4 surfaceOverlap(vec4 inColour, vec2 fragCoord)
{
	if (inColour.a > 0.0)
	{
		vec4 SurfaceColour = texture(u_surface, fragCoord.xy * u_mapSizeSurf);
		if (SurfaceColour.a > 0.0)
		{
			return SurfaceColour;
		}
	}

	return inColour;
}


// Palette Swap
void fragment() {
	vec4 OutColour = texture(TEXTURE, u_parallaxActive ? parallax(UV, TEXTURE_PIXEL_SIZE) : UV);

	if (OutColour == Alpha)
	{
		OutColour = vec4(0.0);
	}
	
	if (u_bound > FRAGCOORD.y)
	{
		// Based on GLSL shader code by Pixelated Pope
		for (float i = u_UVs1.y; i < u_UVs1.z; i += u_texelSize1.y) 
       	{
            vec2 TestPosition = vec2(u_UVs1.x, i);
           	if (texture(u_texture1, TestPosition) == OutColour)
            {
	            float Index = u_index1[int((i - u_UVs1.y) / u_texelSize1.y)];
	            TestPosition.x  += u_texelSize1.x * floor(Index + 1.0);
				OutColour = mix(texture(u_texture1, vec2(TestPosition.x - u_texelSize1.x, TestPosition.y)), 
					texture(u_texture1, TestPosition), fract(Index));
				break;
            }
        }
	}
	else
	{
		for (float i = u_UVs2.y; i < u_UVs2.z; i += u_texelSize2.y) 
        {
            vec2 TestPosition = vec2(u_UVs2.x, i);
            if (texture(u_texture2, TestPosition) == OutColour)
            {
	            float Index = u_index2[int((i - u_UVs2.y) / u_texelSize2.y)];
	           	TestPosition.x  += u_texelSize2.x * floor(Index + 1.0);
				OutColour = mix(texture(u_texture2, vec2(TestPosition.x - u_texelSize2.x, TestPosition.y)), 
					texture(u_texture2, TestPosition), fract(Index));
				break;
            }
        }
	}
	
	if (u_overlapActive)
	{
		OutColour = surfaceOverlap(OutColour, FRAGCOORD.xy);
	}
	
	COLOR = OutColour * v_vColour;
}
