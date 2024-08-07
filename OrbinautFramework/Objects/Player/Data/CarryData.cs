using Godot;

namespace OrbinautFramework3.Objects.Player.Data;

public class CarryData
{
    public ICarryTarget Target { get; set; }
    public float Timer { get; set; }
    public Vector2 TargetPosition { get; set; }
}
