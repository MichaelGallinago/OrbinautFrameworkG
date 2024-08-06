using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct HammerDash : IAction
{
    public PlayerData Data { private get; init; }
    
    public void Perform()
    {
        // Note that ACTION_HAMMERDASH is used for movement logic only so the respective
        // animation isn't cleared alongside the action flag. All checks for Hammer Dash should refer to its animation
		
        if (!Input.Down.Abc)
        {
            Action = Actions.None;
            return;
        }
		
        ActionValue += Scene.Local.ProcessSpeed;
        if (ActionValue >= 60f)
        {
            Action = Actions.None;
            return;
        }
		
        // Air movement isn't overwritten completely, refer to ProcessMovementAir()
        if (!IsGrounded) return;
		
        if (GroundSpeed == 0f || SetPushAnimationBy != null || MathF.Cos(Mathf.DegToRad(Angle)) <= 0f)
        {
            Action = Actions.None;
        }

        TurnAround();
		
        Velocity.SetDirectionalValue(GroundSpeed, Angle);
    }

    private void TurnAround()
    {
        if (Input.Press.Left && GroundSpeed > 0f || Input.Press.Right && GroundSpeed < 0f)
        {
            Facing = (Constants.Direction)(-(int)Facing);
            GroundSpeed.Value = -GroundSpeed;
        }
    }
}