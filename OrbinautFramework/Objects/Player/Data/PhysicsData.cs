using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class PhysicsData
{
    public bool IsForcedSpin { get; set; }
    public float GroundLockTimer { get; set; }
    public bool IsAirLock { get; set; }
    public bool IsControlRoutineEnabled { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsSpinning { get; set; }
    public bool IsJumping { get; set; }
    
    public float Gravity { get; set; }
    public Velocity Velocity { get; } = new();
    public AcceleratedValue GroundSpeed { get; } = new();

    public void Init()
    {
        IsJumping = false;
        IsGrounded = true;
        IsSpinning = false;
        IsForcedSpin = false;
        IsControlRoutineEnabled = true;
        
        Gravity = GravityType.Default;
        Velocity.Vector = Vector2.Zero;
        GroundSpeed.Value = 0f;
        GroundLockTimer = 0f;
    }
    
    public void ResetGravity(bool isUnderwater)
    {
        Gravity = isUnderwater ? GravityType.Underwater : GravityType.Default;
    }
}
