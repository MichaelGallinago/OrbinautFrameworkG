using Godot;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Position(PlayerData data, IPlayerLogic logic)
{
    public void Update()
    {
        if (logic.Action == States.Carried) return;
		
        if (data.Collision.IsStickToConvex)
        {
            data.Movement.Velocity.Clamp(-16f * Vector2.One, 16f * Vector2.One);
        }
		
        data.Node.Position = data.Movement.Velocity.CalculateNewPosition(data.Node.Position);
        data.Movement.Velocity.ResetInstantVelocity();
		
        if (data.Movement.IsGrounded) return;
        data.Movement.Velocity.AccelerationY = data.Movement.Gravity;
    }
}
