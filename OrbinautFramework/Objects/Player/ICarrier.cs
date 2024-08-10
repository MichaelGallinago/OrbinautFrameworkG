using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface ICarrier
{
    Velocity Velocity { get; }
    ActionFsm.States State { get; }
    Vector2 Position { get; }
    Constants.Direction Facing { get; }
    float CarryTimer { get; set; }
    ICarryTarget CarryTarget { get; set; }
    Vector2 CarryTargetPosition { get; set; }

    private void Carry()
    {
        CarryTarget.CarryTarget.OnAttached(this);
    }
}