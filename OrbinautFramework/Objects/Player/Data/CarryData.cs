using Godot;

namespace OrbinautFramework3.Objects.Player.Data;

public class CarryData
{
    public float Timer { get; set; }
    public ICarryTarget Target { get; set; }
    public Vector2 TargetPosition { get; set; }

    public void Init()
    {
        Timer = 0f;
        Target = null;
        TargetPosition = Vector2.Zero;
    }
}
