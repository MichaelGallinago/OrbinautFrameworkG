using Godot;

namespace OrbinautFramework3.Objects.Player;

public class CarryData
{
    public ICarried Target { get; set; }
    public float Timer { get; set; }
    public Vector2 TargetPosition { get; set; }
}