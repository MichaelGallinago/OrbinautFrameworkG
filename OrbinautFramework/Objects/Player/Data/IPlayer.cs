using Godot;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayer
{
    Vector2 Position { get; set; }
    Vector2 Scale { get; set; }
    float RotationDegrees { get; set; }
    float Rotation { get; set; }
    bool Visible { get; set; }
    public ShieldContainer Shield { get; }
}
