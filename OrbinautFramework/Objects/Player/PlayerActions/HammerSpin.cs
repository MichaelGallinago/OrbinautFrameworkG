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
    
    public States Perform()
    {
        if (data.Movement.IsGrounded) return States.HammerSpin;
        
        if (data.Input.Down.Abc)
        {
            Charge();
            return States.HammerSpin;
        }

        if (_charge >= DropDash.MaxCharge)
        {
            data.Visual.Animation = Animations.Spin;
            return States.None;
        }
        
        _charge = 0f; 
        return States.HammerSpin;
    }
    
    public States OnLand() => _charge < DropDash.MaxCharge ? States.Default : States.HammerDash;

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
