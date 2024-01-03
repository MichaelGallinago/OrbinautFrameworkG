using Godot;

namespace OrbinautFramework3.Objects.Player;

public record TailAnimationData
(
    Animations AnimationType,
    Vector2 Scale,
    Vector2 Speed,
    float GroundSpeed,
    bool IsGrounded,
    bool IsSpinning,
    float Angle,
    float VisualAngle
);