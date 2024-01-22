using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    int ZIndex { get; }
    bool IsDead { get; }
    Speed Speed { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    AcceleratedValue GroundSpeed { get; }
    BaseObject OnObject { get; }
    bool ObjectInteraction { get; }
}