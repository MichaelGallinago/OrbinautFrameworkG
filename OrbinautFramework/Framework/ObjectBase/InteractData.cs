using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public record struct InteractData(
    bool IsInteract, 
    Vector2I Radius, 
    Vector2I Offset, 
    Vector2I RadiusExtra, 
    Vector2I OffsetExtra
);
    