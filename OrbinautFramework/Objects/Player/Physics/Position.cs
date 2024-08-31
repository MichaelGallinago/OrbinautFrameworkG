using Godot;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Position(PlayerData data)
{
    public void UpdateAir()
    {
        Update();

        data.Movement.Velocity.AccelerationY = data.Movement.Gravity;
    }
    
    public void UpdateGround() => Update();
    
    private void Update()
    {
        if (data.Collision.IsStickToConvex)
        {
            data.Movement.Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
        }
		
        data.Node.Position = data.Movement.Velocity.CalculateNewPosition(data.Node.Position);
        data.Movement.Velocity.ResetInstantVelocity();
    }
}
