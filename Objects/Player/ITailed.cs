using Godot;

namespace OrbinautFramework3.Objects.Player;

public interface ITailed
{
    Vector2 Scale { get; }
    Vector2 Speed { get; }
    float GroundSpeed { get; }
    bool IsGrounded { get; }
    bool IsSpinning { get; }
    float Angle { get; }
    float VisualAngle { get; }
    Animations Animation { get; }
}