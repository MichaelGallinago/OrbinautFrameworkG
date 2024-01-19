using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface IAnimatedPlayer
{
    Types Type { get; }
    bool IsSuper { get; }
    Speed Speed { get; }
    float GroundSpeed { get; }
    float ActionValue { get; }
    ICarried CarryTarget { get; }
    Constants.Direction Facing { get; }
    Animations Animation { get; set; }
    int? OverrideAnimationFrame { get; set; }
    bool IsAnimationFrameChanged { get; set; }
}