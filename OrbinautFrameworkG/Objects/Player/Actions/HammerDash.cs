using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerDash(PlayerData data)
{
    private float _timer;

    public void Enter()
    {
        data.Sprite.Animation = Animations.HammerDash;
        data.Movement.GroundSpeed = 6f * (float)data.Visual.Facing;
        
        if (data.Super.IsSuper && data.Node.IsCameraTarget(out ICamera camera))
        {
            camera.SetShakeTimer(6f);
        }
		
        AudioPlayer.Sound.Stop(SoundStorage.Charge3);
        AudioPlayer.Sound.Play(SoundStorage.Release);

        Perform();
    }
    
    public States Perform()
    {
        // Note that the animation IS NOT cleared, so all Hammer Dash checks are referring to ANI_HAMMERDASH
        if (data.Movement.GroundSpeed == 0f && _timer > 0f) return States.Default;
        
        _timer += Scene.Instance.Speed;
        if (_timer >= 60f) return States.Default;

        if (!data.Input.Down.Aby || data.Visual.SetPushBy != null) return States.Default;
        if (data.Movement.Angle is >= 90f and <= 270f) return States.Default;
        
        TurnAround();
        SetSpeedAndVelocity();
        
        return States.HammerDash;
    }

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
        if (!data.Movement.IsGrounded) return;
        
        const float hammerDashSpeed = 6f;
        data.Movement.GroundSpeed = hammerDashSpeed * (float)data.Visual.Facing;
        data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }
}
