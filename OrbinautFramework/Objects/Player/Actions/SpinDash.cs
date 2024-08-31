using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Actions;

[FsmSourceGenerator.FsmState("Action")]
public struct SpinDash(PlayerData data)
{
	private float _charge = 0f;
	private float _soundPitch = 1f;
	
	public void Enter()
	{
		data.Sprite.Animation = Animations.SpinDash;
		data.Movement.Velocity.Vector = Vector2.Zero;
		
		// TODO: SpinDash dust 
		//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.Charge);
	}
    
    public States Perform()
    {
	    if (!data.Movement.IsGrounded) return States.SpinDash;
	    
	    if (data.Input.Down.Down)
	    {
		    Charge();
		    return States.SpinDash;
	    }

	    Release();
	    return States.Default;
    }

    private void Release()
    {
	    data.Node.SetCameraDelayX(16f);
    	
	    data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
	    data.Collision.Radius = data.Collision.RadiusSpin;
	    data.Sprite.Animation = Animations.Spin;
	    data.Movement.IsSpinning = true;

	    float baseSpeed = data.Super.IsSuper ? 11f : 8f;
	    data.Movement.GroundSpeed.Value = (baseSpeed + MathF.Round(_charge) / 2f) * (float)data.Visual.Facing;
    	
	    AudioPlayer.Sound.Stop(SoundStorage.Charge);
	    AudioPlayer.Sound.Play(SoundStorage.Release);
    	
	    data.Movement.IsCorePhysicsSkipped = true;
	    
	    if (!SharedData.FixDashRelease) return;
	    data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }

    private void Charge()
    {
    	if (!data.Input.Press.Abc)
	    {
		    //TODO: fix Scene.Instance.Speed somehow (difficulty: extreme demon)
		    _charge -= MathF.Floor(_charge * 8f) / 256f * Scene.Instance.Speed;
    		return;
    	}
    	
	    _charge = Math.Min(_charge + 2f, 8f);

	    bool changePitch = AudioPlayer.Sound.IsPlaying(SoundStorage.Charge) && _charge > 0f;
    	_soundPitch = changePitch ? Math.Min(_soundPitch + 0.1f * Scene.Instance.Speed, 1.5f) : 1f;
    			
    	AudioPlayer.Sound.PlayPitched(SoundStorage.Charge, _soundPitch);
    	data.Visual.OverrideFrame = 0;
    }
}
