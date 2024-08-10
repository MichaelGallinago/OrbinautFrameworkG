using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerDash(PlayerData data)
{
    private float _timer;
    
    public void Perform()
    {
        // Note that ACTION_HAMMERDASH is used for movement logic only so the respective animation
        // is NOT cleared alongside the action flag. All checks for a Hammer Dash action should refer to its animation
		
        if (!data.Input.Down.Abc)
        {
            data.State = States.Default;
            return;
        }
		
        _timer += Scene.Instance.ProcessSpeed;
        if (_timer >= 60f)
        {
            data.State = States.Default;
            return;
        }

        if (data.Movement.GroundSpeed == 0f || data.Visual.SetPushBy != null ||
            MathF.Cos(Mathf.DegToRad(data.Movement.Angle)) <= 0f)
        {
            data.State = States.Default;
        }
		
        // Overwrite ground movement. Air movement is not overwritten completely
        if (!data.Movement.IsGrounded) return;

        TurnAround();
		
        data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }

    private void TurnAround()
    {
        if ((!data.Input.Press.Left || data.Movement.GroundSpeed <= 0f) &&
            (!data.Input.Press.Right || data.Movement.GroundSpeed >= 0f)) return;
        
        data.Visual.Facing = (Constants.Direction)(-(int)data.Visual.Facing);
        data.Movement.GroundSpeed.Value = -data.Movement.GroundSpeed;
    }
}