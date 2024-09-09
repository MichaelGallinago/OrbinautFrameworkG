using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface ICarryTarget : IPosition
{
    Constants.Direction Facing { get; set; }
    Vector2 Velocity { get; set; }
    Vector2 Scale { get; set; }
    bool IsFree { get; }
    void OnFree();
}
