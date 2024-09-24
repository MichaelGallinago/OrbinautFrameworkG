using Godot;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Physics;

public readonly struct Position(PlayerData data)
{
    private const float VelocityLimit = 16f;
    private static readonly Vector2 MinimalVelocity = -VelocityLimit * Vector2.One;
    private static readonly Vector2 MaximalVelocity =  VelocityLimit * Vector2.One;
    
    public void UpdateAir()
    {
        UpdateGround();
        data.Movement.Velocity.Y.AddAcceleration(data.Movement.Gravity);
    }
    
    public void UpdateGround()
    {
        MovementData movement = data.Movement;
        if (data.Collision.IsStickToConvex)
        {
            movement.Velocity.Clamp(MinimalVelocity, MaximalVelocity);
        }
		
        movement.Position += movement.Velocity.ValueDelta;
        movement.Velocity.ResetInstanceValue();
    }
}
