using Godot;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Position(PlayerData data)
{
    public void Update()
    {
        if (data.State == States.Carried) return;
		
        if (data.Collision.IsStickToConvex)
        {
            data.Movement.Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
        }
		
        data.PlayerNode.Position = data.Movement.Velocity.CalculateNewPosition(data.PlayerNode.Position);
        data.Movement.Velocity.ResetInstantVelocity();
		
        if (data.Movement.IsGrounded) return;
        data.Movement.Velocity.AccelerationY = data.Movement.Gravity;
    }
}