using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public record struct HitBox(
    bool IsInteract, Vector2I Radius, Vector2I Offset, Vector2I RadiusExtra, Vector2I OffsetExtra)
{
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
    