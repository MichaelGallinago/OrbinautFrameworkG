using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public record PlayerAnimationData
(
    Types Type, 
    Constants.Direction Facing, 
    bool IsSuper, 
    float GroundSpeed, 
    Vector2 Speed,
    float ActionValue,
    Player CarryTarget
);
