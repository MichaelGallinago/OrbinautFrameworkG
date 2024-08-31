using Godot;

namespace OrbinautFramework3.Objects.Player.Data;

public class CarryData
{
    public float Cooldown { get; set; }
    public ICarryTarget Target { get; set; }
    public Vector2 TargetPosition { get; set; }
    
    public void Init()
    {
        Cooldown = 0f;
        Target = null;
        TargetPosition = Vector2.Zero;
    }
}
