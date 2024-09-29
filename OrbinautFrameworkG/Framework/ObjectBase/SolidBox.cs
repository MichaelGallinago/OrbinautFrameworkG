using System.Linq;
using Godot;
using Godot.Collections;

namespace OrbinautFrameworkG.Framework.ObjectBase;

[GlobalClass]
public partial class SolidBox : Resource
{
    [Export] public bool NoBalance { get; set; }
    [Export] public Vector2I Radius { get; private set; }
    [Export] public Vector2I Offset { get; private set; }

    [Export] private Array<short> HeightMapArray //TODO: remove this?
    {
        get => new(HeightMap);
        set => HeightMap = value.ToArray();
    }

    public short[] HeightMap { get; private set; }

    public SolidBox() {}
    public SolidBox(bool noBalance, Vector2I radius, Vector2I offset, short[] heightMap) : this()
    {
        NoBalance = noBalance;
        Radius = radius;
        Offset = offset;
        HeightMap = heightMap;
    }

    public void Set(Vector2I newRadius, Vector2I newOffset = default)
    {
        Offset = newOffset;
        Radius = newRadius;
        HeightMap = null;
    }
	
    public void Set(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        Radius = new Vector2I(radiusX, radiusY);
        Offset = new Vector2I(offsetX, offsetY);
        HeightMap = null;
    }
}
