using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class MovementData
{
    public float Angle { get; set; }
    public float Gravity { get; set; }
    public bool IsAirLock { get; set; }
    public bool IsJumping { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsSpinning { get; set; }
    public bool IsForcedRoll { get; set; }
    public float GroundLockTimer { get; set; }
    public bool IsCorePhysicsSkipped { get; set; }
    
    public Velocity Velocity { get; } = new();
    public AcceleratedValue GroundSpeed { get; } = new();
    
    public void Init()
    {
        IsGrounded = true;
        IsJumping = false;
        IsForcedRoll = false;
        
        Gravity = GravityType.Default;
        Velocity.Vector = Vector2.Zero;
        GroundSpeed.Value = 0f;
        GroundLockTimer = 0f;
        Angle = 0f;
    }
}
