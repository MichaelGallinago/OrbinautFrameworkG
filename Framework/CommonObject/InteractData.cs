using Godot;

namespace OrbinautFramework3.Framework.CommonObject;

public struct InteractData
{
    public bool IsInteract;
    public Vector2I Radius;
    public Vector2I Offset;
    public Vector2I RadiusExtra;
    public Vector2I OffsetExtra;

    public InteractData()
    {
        IsInteract = true;
        Radius = default;
        Offset = default;
        RadiusExtra = default;
        OffsetExtra = default;
    }
}