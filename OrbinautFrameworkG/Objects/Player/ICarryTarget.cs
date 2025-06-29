using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player;

public interface ICarryTarget : IPosition
{
    Constants.Direction Facing { get; set; }
    Vector2 Velocity { get; set; }
    Vector2 Scale { get; set; }
    bool TryFree(out float cooldown);
    void Collide();
    void OnFree();
}
