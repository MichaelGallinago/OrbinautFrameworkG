using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct ClimbWallJump(PlayerData data)
{
    public void Enter()
    {
        data.ResetGravity();
		
        data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
		
        var velocity = new Vector2(3.5f * (float)data.Visual.Facing, data.Physics.MinimalJumpSpeed);
        data.Movement.Velocity.Vector = velocity;
        
        data.State = States.Jump;
    }
}