using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public partial class HitBox : Resource
{
    [Export] public bool IsInteract { get; private set; }
    [Export] public Vector2I Radius { get; private set; }
    [Export] public Vector2I Offset { get; private set; }
    [Export] public Vector2I RadiusExtra { get; private set; }
    [Export] public Vector2I OffsetExtra { get; private set; }

    public HitBox(bool isInteract, Vector2I radius, Vector2I offset, Vector2I radiusExtra, Vector2I offsetExtra)
    {
        IsInteract = isInteract;
        Radius = radius;
        Offset = offset;
        RadiusExtra = radiusExtra;
        OffsetExtra = offsetExtra;
    }
    
    public HitBox() : this(false, default, default, default, default) {}
    
    public void Set(Vector2I radius, Vector2I offset = default)
    {
        Radius = radius;
        Offset = offset;
    }
	
    public void Set(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        Radius = new Vector2I(radiusX, radiusY);
        Offset = new Vector2I(offsetX, offsetY);
    }

    public void SetExtra(Vector2I radius, Vector2I offset = default)
    {
        OffsetExtra = offset;
        RadiusExtra = radius;
    }
	
    public void SetExtra(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        RadiusExtra = new Vector2I(radiusX, radiusY);
        OffsetExtra = new Vector2I(offsetX, offsetY);
    }
}
    