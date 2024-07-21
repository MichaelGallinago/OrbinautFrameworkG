using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface ITailed
{
    Velocity Velocity { get; }
    Vector2 Scale { get; }
    bool IsGrounded { get; }
    bool IsSpinning { get; }
    float VisualAngle { get; }
    Animations Animation { get; }
    float RotationDegrees { get; }
    AcceleratedValue GroundSpeed { get; }
}
