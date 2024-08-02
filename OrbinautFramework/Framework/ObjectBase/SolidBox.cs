using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public partial class SolidBox : Resource
{
    public bool NoBalance { get; private set; }
    public Vector2I Radius { get; private set; }
    public Vector2I Offset { get; private set; }
    public short[] HeightMap { get; private set; }
    
    public SolidBox() : this(false, default, default, null) {}
    
    public SolidBox(bool isInteract, Vector2I radius, Vector2I offset, short[] heightMap)
    {
        NoBalance = isInteract;
        Radius = radius;
        Offset = offset;
        HeightMap = heightMap;
    }
    
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
