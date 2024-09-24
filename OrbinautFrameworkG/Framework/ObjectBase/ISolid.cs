using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase;

public interface ISolid
{
    SolidBox SolidBox { get; }
    Vector2 Position { get; }
    Vector2 PreviousPosition { get; }
    Vector2 Scale { get; }

    bool IsInstanceValid();
}
