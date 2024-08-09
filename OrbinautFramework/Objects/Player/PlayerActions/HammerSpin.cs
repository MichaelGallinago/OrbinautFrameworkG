using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerSpin(PlayerData data)
{
    public void Perform()
    {
        if (PlayerNode.Data.IsGrounded) return;
        
        if (PlayerNode.Data.Input.Down.Abc)
        {
            Charge();
            return;
        }
		
        switch (PlayerNode.Data.ActionValue)
        {
            case <= 0f: return;
			
            case >= DropDash.MaxCharge:
                PlayerNode.Data.Animation = Animations.Spin;
                PlayerNode.Data.Action = Actions.HammerSpinCancel;
                break;
        }

        PlayerNode.Data.ActionValue = 0f;
    }
    
    public void OnLand()
    {
        if (ActionValue < DropDash.MaxCharge) return;

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
        PlayerNode.Data.ActionValue += Scene.Instance.ProcessSpeed;
        if (ActionValue >= DropDash.MaxCharge)
        {
            AudioPlayer.Sound.Play(SoundStorage.Charge3);
        }

        IsAirLock = false;
    }
}