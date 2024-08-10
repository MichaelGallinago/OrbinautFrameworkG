using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("data.State")]
public struct HammerDash(PlayerData data)
{
    public void Perform()
    {
        // Note that ACTION_HAMMERDASH is used for movement logic only so the respective
        // animation isn't cleared alongside the action flag. All checks for Hammer Dash should refer to its animation
		
        if (!data.Input.Down.Abc)
        {
            data.State = States.Default;
            return;
        }
		
        ActionValue += Scene.Instance.ProcessSpeed;
        if (ActionValue >= 60f)
        {
            data.State = States.Default;
            return;
        }
		
        // Air movement isn't overwritten completely, refer to ProcessMovementAir()
        if (!data.Physics.IsGrounded) return;
		
        if (data.Physics.GroundSpeed == 0f || data.Visual.SetPushBy != null || 
            MathF.Cos(Mathf.DegToRad(data.Rotation.Angle)) <= 0f)
        {
            data.State = States.Default;
        }

        TurnAround();
		
        data.Physics.Velocity.SetDirectionalValue(data.Physics.GroundSpeed, data.Rotation.Angle);
    }

    private void TurnAround()
    {
        if ((!data.Input.Press.Left || data.Physics.GroundSpeed <= 0f) &&
            (!data.Input.Press.Right || data.Physics.GroundSpeed >= 0f)) return;
        
        data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
        data.Physics.GroundSpeed.Value = -data.Physics.GroundSpeed;
    }
}