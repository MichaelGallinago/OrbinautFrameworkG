using Godot;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Position(PlayerData data)
{
    private const float VelocityLimit = 16f;
    private static readonly Vector2 MinimalVelocity = -VelocityLimit * Vector2.One;
    private static readonly Vector2 MaximalVelocity =  VelocityLimit * Vector2.One;
    
    public void UpdateAir()
    {
        UpdateGround();
        data.Movement.Velocity.AccelerationY = data.Movement.Gravity;
    }
    
    public void UpdateGround()
    {
        if (data.Collision.IsStickToConvex)
        {
            data.Movement.Velocity.Clamp(MinimalVelocity, MaximalVelocity);
        }
		
        data.Node.Position = data.Movement.Velocity.CalculateNewPosition(data.Node.Position);
        data.Movement.Velocity.ResetInstantVelocity();
    }
}
