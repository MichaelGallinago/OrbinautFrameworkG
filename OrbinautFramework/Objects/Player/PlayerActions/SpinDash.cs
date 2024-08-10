using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct SpinDash(PlayerData data)
{
	private float _charge;
	private float _soundPitch;
    
    public bool Perform()
    {
	    if (!SharedData.SpinDash || !data.Movement.IsGrounded) return false;
    	
	    if (Start()) return false;
    	
	    // Continue if Spin Dash is being performed
	    if (data.State != States.SpinDash) return false;
    	
	    if (Charge()) return false;

	    Release();
	    return true;
    }

    private void Release()
    {
	    data.SetCameraDelayX(16f);
    	
	    data.PlayerNode.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
	    data.Visual.Animation = Animations.Spin;
	    data.State = States.None;
	    data.Collision.Radius = data.Collision.RadiusSpin;
	    data.Movement.IsSpinning = true;
    	
	    data.Movement.GroundSpeed.Value = ((data.Super.IsSuper ? 11f : 8f) + MathF.Round(_charge) / 2f) * (float)data.Visual.Facing;
    	
	    AudioPlayer.Sound.Stop(SoundStorage.Charge);
	    AudioPlayer.Sound.Play(SoundStorage.Release);
    	
	    if (!SharedData.FixDashRelease) return;
	    data.Movement.Velocity.SetDirectionalValue(data.Movement.GroundSpeed, data.Movement.Angle);
    }

    private bool Start()
    {
    	if (data.State != States.Default) return false;
	    if (data.Visual.Animation is not (Animations.Duck or Animations.GlideLand)) return false;
    	if (!data.Input.Press.Abc || !data.Input.Down.Down) return true;
    	
    	data.Visual.Animation = Animations.SpinDash;
    	data.State = States.SpinDash;
	    _charge = 0f;
    	_soundPitch = 1f; 
	    data.Movement.Velocity.Vector = Vector2.Zero;
    		
    	// TODO: SpinDash dust 
    	//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
    	AudioPlayer.Sound.Play(SoundStorage.Charge);
    	
    	return true;
    }

    private bool Charge()
    {
    	if (!data.Input.Down.Down) return false;
    	
    	if (!data.Input.Press.Abc)
    	{
    		//TODO: check math with ProcessSpeed
		    _charge -= MathF.Floor(_charge * 8f) / 256f * Scene.Instance.ProcessSpeed;
    		return true;
    	}
    	
	    _charge = Math.Min(_charge + 2f, 8f);

	    bool changePitch = AudioPlayer.Sound.IsPlaying(SoundStorage.Charge) && _charge > 0f;
    	_soundPitch = changePitch ? Math.Min(_soundPitch + 0.1f, 1.5f) : 1f;
    			
    	AudioPlayer.Sound.PlayPitched(SoundStorage.Charge, _soundPitch);
    	data.Visual.OverrideFrame = 0;
    	
    	return true;
    }
}
