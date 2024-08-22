using Godot;
using Godot.Collections;

namespace OrbinautFramework3.Framework.ObjectBase;

public partial class SolidBox(bool isInteract, Vector2I radius, Vector2I offset, Array<short> heightMap) : Resource
{
    [Export] public bool NoBalance { get; set; } = isInteract;
    [Export] public Vector2I Radius { get; private set; } = radius;
    [Export] public Vector2I Offset { get; private set; } = offset;
    [Export] public Array<short> HeightMap { get; private set; } = heightMap;

    public SolidBox() : this(false, default, default, null) {}

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
