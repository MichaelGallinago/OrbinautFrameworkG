shader_type canvas_item;

// Set to 1 to significantly speed up 
// color replacement (requires a sorted palette)
#define BINARY_SEARCH 0

// Also used in c_engine -> Create Event -> Initialise Palette
#define PALETTE_LIMIT 256

#define ALPHA vec4(1.0, 0.0, 1.0, 1.0)

uniform float u_bound;

uniform sampler2D u_texture1;
uniform vec3	  u_UVs1;
uniform vec2	  u_texelSize1;
uniform float     u_index1[PALETTE_LIMIT];

uniform sampler2D u_texture2;
uniform vec3      u_UVs2;
uniform vec2      u_texelSize2;
uniform float     u_index2[PALETTE_LIMIT];

uniform bool  u_parallaxActive;
uniform vec4  u_offset;
uniform vec2  u_size;
uniform float u_scaling;
uniform float u_inclineHeight;
uniform float u_inclineStep;
	
uniform bool u_overlapActive;
uniform vec2 u_mapSizeSurf;
uniform sampler2D u_surface;

// Background Parallax
vec2 parallax(vec2 uv, vec2 texturePixelSize) 
{
	// Get the position on the sprite
	vec2 Position = uv / texturePixelSize;
	
	// Set the pixel offset (can be negative)
	vec2 Offset = u_offset.xy;
	
	// Adjust the pixel offset based on height and incline step
	if (u_inclineHeight != 0.0) 
	{
		Offset.x *= floor((Position.y / u_scaling - u_size.y) 
			/ u_inclineHeight + 1.0) * u_inclineStep + 1.0;
	}
	

	// Normalise the offset value to ensure
	// the final pixel position is within u_width
	Offset.xy = mod(mod(floor(Offset.xy) + Position - u_offset.zw, u_size)
		+ u_size, u_size) - Position;
		
	// Return the new pixel position with the offset converted to uv coordinates
	return uv + Offset * texturePixelSize;
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

#if (BINARY_SEARCH > 0)

bool colorIsGreater(vec4 first, vec4 second)
{
    vec4 Diff = sign(first - second) * vec4(8.0, 4.0, 2.0, 1.0);
    return Diff.r + Diff.g + Diff.b + Diff.a >= 0.0;
}

vec4 replaceColour(vec4 target, sampler2D tex, vec3 UVs, 
	vec2 texelSize, float index[PALETTE_LIMIT])
{	
	int Left = 0;
	int Right = int((UVs.z - UVs.y) / texelSize.y) - 1;

	while (Left <= Right)
	{
		int Mid = (Left + Right) / 2;
		vec2 TestPosition = vec2(UVs.x, float(Mid) * texelSize.y + UVs.y);
		vec4 CompareColour = texture2D(tex, TestPosition);
		if (CompareColour == target)
		{
			float Index = index[Mid];
	        TestPosition.x += texelSize.x * floor(Index + 1.0);
			return mix(texture2D(tex, 
				vec2(TestPosition.x - texelSize.x, TestPosition.y)), 
				texture2D(tex, TestPosition), fract(Index));
		}
		else if (colorIsGreater(target, CompareColour))
		{
		    Left = Mid + 1;
		}
		else
		{
		    Right = Mid - 1;
		}
	}

	return target;
}

#else

vec4 replaceColour(vec4 target, sampler2D tex, vec3 UVs, 
	vec2 texelSize, float index[PALETTE_LIMIT])
{
	// Based on GLSL shader code by Pixelated Pope
	for (float i = UVs.y; i < UVs.z; i += texelSize.y) 
	{
	    vec2 TestPosition = vec2(UVs.x, i);
	    if (texture(tex, TestPosition) == target)
	    {
		    float Index = index[int((i - UVs.y) / texelSize.y)];
		    TestPosition.x += texelSize.x * floor(Index + 1.0);
			return mix(texture(tex, 
				vec2(TestPosition.x - texelSize.x, TestPosition.y)), 
				texture(tex, TestPosition), fract(Index));
	    }
	}
	
	return target;
}

#endif

// Palette Swap
void fragment() {
	vec4 OutColour = texture(TEXTURE, u_parallaxActive ? 
		parallax(UV, TEXTURE_PIXEL_SIZE) : UV);

	if (OutColour == ALPHA)
	{
		OutColour = vec4(0.0);
	}
	
	if (u_bound > FRAGCOORD.y)
	{
		OutColour = replaceColour(
			OutColour, u_texture1, u_UVs1, u_texelSize1, u_index1);
	}
	else
	{
		OutColour = replaceColour(
			OutColour, u_texture2, u_UVs2, u_texelSize2, u_index2);
	}
	
	if (u_overlapActive)
	{
		OutColour = surfaceOverlap(OutColour, FRAGCOORD.xy);
	}
	
	COLOR = OutColour;
}
