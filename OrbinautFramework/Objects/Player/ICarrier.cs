using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface ICarrier
{
    Velocity Velocity { get; }
    ActionFsm.States State { get; }
    Vector2 Position { get; }
    Constants.Direction Facing { get; }
    float Cooldown { get; set; }
    ICarryTarget Target { get; set; }
    Vector2 TargetPosition { get; set; }
}