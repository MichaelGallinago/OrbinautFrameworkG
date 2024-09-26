using Godot;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Data;

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

    public ref AcceleratedVector2 Velocity => ref _velocity;
    private AcceleratedVector2 _velocity;
    
    public ref AcceleratedValue GroundSpeed => ref _groundSpeed;
    private AcceleratedValue _groundSpeed;

    public ref Vector2 Position => ref _position;
    private Vector2 _position;
    
    public void Init(Vector2 position)
    {
        IsGrounded = true;
        IsJumping = false;
        IsForcedRoll = false;
        
        _position = position;
        
        Gravity = GravityType.Default;
        Velocity = Vector2.Zero;
        GroundLockTimer = 0f;
        GroundSpeed = 0f;
        Angle = 0f;
    }
}
