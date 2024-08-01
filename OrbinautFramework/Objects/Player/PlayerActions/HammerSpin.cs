using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct HammerSpin : IAction
{
    public Player Player { private get; init; }
    
    public void Perform()
    {
        if (Player.Data.IsGrounded) return;
        
        if (Player.Data.Input.Down.Abc)
        {
            Charge();
            return;
        }
		
        switch (Player.Data.ActionValue)
        {
            case <= 0f: return;
			
            case >= PlayerConstants.MaxDropDashCharge:
                Player.Data.Animation = Animations.Spin;
                Player.Data.Action = Actions.HammerSpinCancel;
                break;
        }

        Player.Data.ActionValue = 0f;
    }
    
    public void Release()
    {
        if (Action != Actions.HammerSpin) return;
        if (ActionValue < MaxDropDashCharge) return;

        Animation = Animations.HammerDash;
        Action = Actions.HammerDash;
        ActionValue = 0f;
        GroundSpeed.Value = 6f * (float)Facing;
		
        if (IsSuper && IsCameraTarget(out ICamera camera))
        {
            camera.SetShakeTimer(6f);
        }
		
        AudioPlayer.Sound.Stop(SoundStorage.Charge3);
        AudioPlayer.Sound.Play(SoundStorage.Release);
    }

    private void Charge()
    {
        Player.Data.ActionValue += Scene.Local.ProcessSpeed;
        if (ActionValue >= MaxDropDashCharge)
        {
            AudioPlayer.Sound.Play(SoundStorage.Charge3);
        }

        IsAirLock = false;
    }
}