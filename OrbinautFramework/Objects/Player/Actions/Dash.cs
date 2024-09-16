using System;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct Dash(PlayerData data, IPlayerLogic logic)
{
	private const float ChargeLimit = 30f;
	
	private float _charge = 0f;
	private float _releaseSpeed = 0f;
	
	public void Enter()
	{
		data.Sprite.Animation = Animations.Move;
		AudioPlayer.Sound.Play(SoundStorage.Charge2);
	}
	
    public States Perform()
    {
	    if (!data.Movement.IsGrounded) return States.Dash;
	    if (logic.ControlType.IsCpu && data.Cpu.InputTimer <= 0f) return States.Dash;
	    
	    if (Charge()) return States.Dash;
	    
	    if (Release())
	    {
		    data.Movement.IsCorePhysicsSkipped = true;
	    }
	    
	    return States.Default;
    }

    public static void Exit() => AudioPlayer.Sound.Stop(SoundStorage.Charge2);
    
    public States OnLand()
    {
	    data.Movement.GroundSpeed = _charge;
	    return States.Default;
	    //TODO: check this
    }

    private bool Charge()
    {
    	if (!data.Input.Down.Up) return false;
    	
    	if (_charge < ChargeLimit)
    	{
    		_charge += Scene.Instance.Speed;
    	}

    	float acceleration = 0.390625f * (float)data.Visual.Facing * Scene.Instance.Speed;
	    float launchSpeed = data.Item.SpeedTimer > 0f || data.Super.IsSuper ? 1.5f : 2f;
    	launchSpeed *= data.Physics.AccelerationTop;
	    
    	_releaseSpeed = Math.Clamp(_releaseSpeed + acceleration, -launchSpeed, launchSpeed);
    	data.Movement.GroundSpeed = _releaseSpeed;
    	return true;
    }

    private bool Release()
    {
    	if (_charge < ChargeLimit)
    	{
    		data.Movement.GroundSpeed = 0f;
    		return false;
    	}

    	data.Node.SetCameraDelayX(16f);
    	
    	AudioPlayer.Sound.Play(SoundStorage.Release2);	
    	
    	if (!Improvements.FixDashRelease) return true;
    	data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    	return true;
    }
}
