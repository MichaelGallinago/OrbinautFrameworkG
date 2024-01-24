using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface ITailed
{
    Vector2 Scale { get; }
    Velocity Velocity { get; }
    bool IsGrounded { get; }
    bool IsSpinning { get; }
    float Angle { get; }
    float VisualAngle { get; }
    Animations Animation { get; }
    AcceleratedValue GroundSpeed { get; }
}