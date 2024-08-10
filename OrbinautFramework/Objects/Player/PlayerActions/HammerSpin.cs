using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerSpin(PlayerData data)
{
    public void Perform()
    {
        if (data.Physics.IsGrounded) return;
        
        if (data.Input.Down.Abc)
        {
            Charge();
            return;
        }
		
        switch (ActionValue)
        {
            case <= 0f: return;
			
            case >= DropDash.MaxCharge:
                data.Visual.Animation = Animations.Spin;
                data.State = States.None;
                break;
        }

        ActionValue = 0f;
    }
    
    public void OnLand()
    {
        if (ActionValue < DropDash.MaxCharge) return;

        data.State = States.HammerDash;
        data.Visual.Animation = Animations.HammerDash;
        data.Physics.GroundSpeed.Value = 6f * (float)data.Visual.Facing;
        ActionValue = 0f;
		
        if (data.Super.IsSuper && data.IsCameraTarget(out ICamera camera))
        {
            camera.SetShakeTimer(6f);
        }
		
        AudioPlayer.Sound.Stop(SoundStorage.Charge3);
        AudioPlayer.Sound.Play(SoundStorage.Release);
    }

    private void Charge()
    {
        ActionValue += Scene.Instance.ProcessSpeed;
        if (ActionValue >= DropDash.MaxCharge)
        {
            AudioPlayer.Sound.Play(SoundStorage.Charge3);
        }

        data.Physics.IsAirLock = false;
    }
}