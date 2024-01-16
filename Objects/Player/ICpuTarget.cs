using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICpuTarget
{
    int ZIndex { get; }
    bool IsDead { get; }
    Vector2 Speed { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    float GroundSpeed { get; }
    BaseObject OnObject { get; }
    bool ObjectInteraction { get; }
}