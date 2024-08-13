using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Dash(PlayerData data)
{
	private const float ChargeLimit = 30f;
	
	private float _charge = 0f;
	private float _releaseSpeed = 0f;
	
	public void Enter()
	{
		data.Visual.Animation = Animations.Move;
    		
		AudioPlayer.Sound.Play(SoundStorage.Charge2);
	}
	
    public bool Perform()
    {
	    if (!SharedData.Dash || data.Node.Type != PlayerNode.Types.Sonic) return false;
	    if (data.Id > 0 && CpuInputTimer <= 0f) return false;
    	
	    if (data.Movement.IsGrounded) return !Charge() && Release();
	    
	    return false;
    }

    public void Exit()
    {
	    AudioPlayer.Sound.Stop(SoundStorage.Charge2);
    }

    private bool Charge()
    {
    	if (!data.Input.Down.Up) return false;
    	
    	if (_charge < ChargeLimit)
    	{
    		_charge += Scene.Instance.ProcessSpeed;
    	}

    	float acceleration = 0.390625f * (float)data.Visual.Facing * Scene.Instance.ProcessSpeed;
	    float launchSpeed = data.Item.SpeedTimer > 0f || data.Super.IsSuper ? 1.5f : 2f;
    	launchSpeed *= data.Physics.AccelerationTop;
	    
    	_releaseSpeed = Math.Clamp(_releaseSpeed + acceleration, -launchSpeed, launchSpeed);
    	data.Movement.GroundSpeed.Value = _releaseSpeed;
    	return true;
    }

    private bool Release()
    {
    	data.State = States.Default;
    	
    	if (_charge < ChargeLimit)
    	{
    		data.Movement.GroundSpeed.Value = 0f;
    		return false;
    	}

    	data.SetCameraDelayX(16f);
    	
    	AudioPlayer.Sound.Play(SoundStorage.Release2);	
    	
    	if (!SharedData.FixDashRelease) return true;
    	data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    	return true;
    }
}
