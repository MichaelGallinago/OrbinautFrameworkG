using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerDash(PlayerData data)
{
    private float _timer;

    public void Enter()
    {
        data.Sprite.Animation = Animations.HammerDash;
        data.Movement.GroundSpeed.Value = 6f * (float)data.Visual.Facing;
        
        if (data.Super.IsSuper && data.IsCameraTarget(out ICamera camera))
        {
            camera.SetShakeTimer(6f);
        }
		
        AudioPlayer.Sound.Stop(SoundStorage.Charge3);
        AudioPlayer.Sound.Play(SoundStorage.Release);

        Perform();
    }
    
    public States Perform()
    {
        // Note that ACTION_HAMMERDASH is used for movement logic only so the respective animation
        // is NOT cleared alongside the action flag. All checks for a Hammer Dash action should refer to its animation
		
        if (!data.Input.Down.Abc) return States.Default;
		
        _timer += Scene.Instance.ProcessSpeed;
        if (_timer >= 60f) return States.Default;

        MovementData movement = data.Movement;
        
        if (movement.GroundSpeed == 0f || data.Visual.SetPushBy != null) return States.Default; 
        if (MathF.Cos(Mathf.DegToRad(movement.Angle)) <= 0f) return States.Default;
        
        TurnAround();
        data.Sprite.Animation = Animations.HammerDash;
        SetSpeedAndVelocity();
        
        return States.HammerDash;
    }

    public States OnLand() => Perform();

    private void TurnAround()
    {
        bool isPositive = data.Visual.Facing == Constants.Direction.Positive;
        if (isPositive && data.Input.Press.Left || !isPositive && data.Input.Press.Right)
        {
            data.Visual.Facing = isPositive ? Constants.Direction.Negative : Constants.Direction.Positive;
        }
    }

    private void SetSpeedAndVelocity()
    {
        const float hammerDashSpeed = 6f;
        data.Movement.GroundSpeed.Value = hammerDashSpeed * (float)data.Visual.Facing;
        
        if (!data.Movement.IsGrounded) return;
        data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }
}
