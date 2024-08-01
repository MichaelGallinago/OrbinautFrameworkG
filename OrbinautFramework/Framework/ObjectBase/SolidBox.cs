using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public record struct SolidBox(bool NoBalance, Vector2I Radius, Vector2I Offset, short[] HeightMap)
{
    public void Set(Vector2I radius, Vector2I offset = default)
    {
        Offset = offset;
        Radius = radius;
        HeightMap = null;
    }
	
    public void Set(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        Radius = new Vector2I(radiusX, radiusY);
        Offset = new Vector2I(offsetX, offsetY);
        HeightMap = null;
    }
}
