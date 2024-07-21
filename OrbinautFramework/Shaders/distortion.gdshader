shader_type canvas_item;

#define WaveLimit 256

uniform vec2 g_WaveY;
uniform vec3 g_Bound;
uniform float g_ScreenWid;
uniform vec2 g_WaveHeight;

uniform float g_WaveData1[WaveLimit];
uniform float g_WaveData2[WaveLimit];

void fragment()
{
	vec2 OutPos = UV;
	if (g_Bound.x < FRAGCOORD.y && g_Bound.z >= FRAGCOORD.y)
	{
		float Offset = g_Bound.y > FRAGCOORD.y ?
			g_WaveData1[int(mod(g_WaveY.x + FRAGCOORD.y, g_WaveHeight.x))] :
			g_WaveData2[int(mod(g_WaveY.y + FRAGCOORD.y, g_WaveHeight.y))];

		OutPos.x -= Offset / g_ScreenWid;
	}
	COLOR = texture(TEXTURE, OutPos);
}
