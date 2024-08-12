using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Dash(PlayerData data)
{
	private const float ChargeLimit = 30f;
	
	private float _charge;
	private float _releaseSpeed;
	
    public bool Perform()
    {
	    if (!SharedData.Dash || data.Node.Type != PlayerNode.Types.Sonic || 
	        data.Id > 0 && CpuInputTimer <= 0f) return false;
    	
	    StartDash();
    	
	    if (data.State == States.Dash && data.Movement.IsGrounded) return !ChargeDash() && ReleaseDash();
    	
	    if (data.State != States.Dash)
	    {
		    AudioPlayer.Sound.Stop(SoundStorage.Charge2);
	    }
	    return false;
    }

    private void StartDash()
    {
    	if (data.State != States.Default || data.Visual.Animation != Animations.LookUp) return;
	    if (!data.Input.Down.Up || !data.Input.Press.Abc) return;
    	
    	data.Visual.Animation = Animations.Move;
	    data.State = States.Dash;
    	_charge = 0f;
    	_releaseSpeed = 0f;
    		
    	AudioPlayer.Sound.Play(SoundStorage.Charge2);
    }

    private bool ChargeDash()
    {
    	if (!data.Input.Down.Up) return false;
    	
    	if (_charge < ChargeLimit)
    	{
    		_charge += Scene.Instance.ProcessSpeed;
    	}

    	float acceleration = 0.390625f * (float)data.Visual.Facing * Scene.Instance.ProcessSpeed;
    	float launchSpeed = 
		    data.Physics.AccelerationTop * (data.Item.SpeedTimer > 0f || data.Super.IsSuper ? 1.5f : 2f);
	    
    	_releaseSpeed = Math.Clamp(_releaseSpeed + acceleration, -launchSpeed, launchSpeed);
    	data.Movement.GroundSpeed.Value = _releaseSpeed;
    	return true;
    }

    private bool ReleaseDash()
    {
    	AudioPlayer.Sound.Stop(SoundStorage.Charge2);
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