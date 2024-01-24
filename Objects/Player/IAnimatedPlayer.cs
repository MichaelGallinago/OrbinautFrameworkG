using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface IAnimatedPlayer
{
    Types Type { get; }
    bool IsSuper { get; }
    Velocity Velocity { get; }
    float ActionValue { get; }
    ICarried CarryTarget { get; }
    Constants.Direction Facing { get; }
    AcceleratedValue GroundSpeed { get; }
    Animations Animation { get; set; }
    int? OverrideAnimationFrame { get; set; }
    bool IsAnimationFrameChanged { get; set; }
}