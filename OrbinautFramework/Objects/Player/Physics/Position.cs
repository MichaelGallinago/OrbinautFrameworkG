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
            data.Physics.Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
        }
		
        data.PlayerNode.Position = data.Physics.Velocity.CalculateNewPosition(data.PlayerNode.Position);
        data.Physics.Velocity.ResetInstantVelocity();
		
        if (data.Physics.IsGrounded) return;
        data.Physics.Velocity.AccelerationY = data.Physics.Gravity;
    }
}