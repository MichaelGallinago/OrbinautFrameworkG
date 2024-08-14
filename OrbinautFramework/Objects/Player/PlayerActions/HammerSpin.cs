using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct HammerSpin(PlayerData data)
{
    private float _charge = 0f;

    public void Enter()
    {
        if (SharedData.NoRollLock)
        {
            data.Movement.IsAirLock = false;
        }
		
        data.Visual.Animation = Animations.HammerSpin;
		
        AudioPlayer.Sound.Play(SoundStorage.Hammer);
    }
    
    public void Perform()
    {
        if (data.Movement.IsGrounded) return;
        
        if (data.Input.Down.Abc)
        {
            Charge();
            return;
        }
		
        switch (_charge)
        {
            case <= 0f: return;
			
            case >= DropDash.MaxCharge:
                data.Visual.Animation = Animations.Spin;
                data.State = States.None;
                break;
        }

        _charge = 0f;
    }
    
    public void OnLand()
    {
        if (_charge < DropDash.MaxCharge) return;
        data.State = States.HammerDash;
    }

    private void Charge()
    {
        _charge += Scene.Instance.ProcessSpeed;
        if (_charge >= DropDash.MaxCharge)
        {
            AudioPlayer.Sound.Play(SoundStorage.Charge3);
        }

        data.Movement.IsAirLock = false;
    }
}
