using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player;

public interface ICarrier
{
    Speed Speed { get; }
    Actions Action { get; }
    Vector2 Position { get; }
    Constants.Direction Facing { get; }
    float CarryTimer { get; set; }
    ICarried CarryTarget { get; set; }
    Vector2 CarryTargetPosition { get; set; }

    private void Carry()
    {
        CarryTarget.OnAttached(this);
    }
}