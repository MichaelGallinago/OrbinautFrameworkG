using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public interface ISolid : IPreviousPosition
{
    SolidBox SolidBox { get; }
    Vector2 Scale { get; }

    bool IsInstanceValid();
}
