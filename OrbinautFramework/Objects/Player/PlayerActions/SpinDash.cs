using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using static OrbinautFramework3.Objects.Player.Data.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct SpinDash
{
	public PlayerData Data { private get; init; }

	private float _charge;
	private float _soundPitch;
    
    public bool Perform()
    {
	    if (!SharedData.SpinDash || !Data.Physics.IsGrounded) return false;
    	
	    if (Start()) return false;
    	
	    // Continue if Spin Dash is being performed
	    if (Data.State != States.SpinDash) return false;
    	
	    if (Charge()) return false;

	    Release();
	    return true;
    }

    private void Release()
    {
	    Data.SetCameraDelayX(16f);
    	
	    Data.PlayerNode.Position += new Vector2(0f, Data.Collision.Radius.Y - Data.Collision.RadiusSpin.Y);
	    Data.Visual.Animation = Animations.Spin;
	    Data.State = States.None;
	    Data.Collision.Radius = Data.Collision.RadiusSpin;
	    Data.Physics.IsSpinning = true;
    	
	    Data.Physics.GroundSpeed.Value = ((Data.Super.IsSuper ? 11f : 8f) + MathF.Round(_charge) / 2f) * (float)Data.Visual.Facing;
    	
	    AudioPlayer.Sound.Stop(SoundStorage.Charge);
	    AudioPlayer.Sound.Play(SoundStorage.Release);
    	
	    if (!SharedData.FixDashRelease) return;
	    Data.Physics.Velocity.SetDirectionalValue(Data.Physics.GroundSpeed, Data.Rotation.Angle);
    }

    private bool Start()
    {
    	if (Data.State != States.Default) return false;
	    if (Data.Visual.Animation is not (Animations.Duck or Animations.GlideLand)) return false;
    	if (!Data.Input.Press.Abc || !Data.Input.Down.Down) return true;
    	
    	Data.Visual.Animation = Animations.SpinDash;
    	Data.State = States.SpinDash;
	    _charge = 0f;
    	_soundPitch = 1f; 
	    Data.Physics.Velocity.Vector = Vector2.Zero;
    		
    	// TODO: SpinDash dust 
    	//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
    	AudioPlayer.Sound.Play(SoundStorage.Charge);
    	
    	return true;
    }

    private bool Charge()
    {
    	if (!Data.Input.Down.Down) return false;
    	
    	if (!Data.Input.Press.Abc)
    	{
    		//TODO: check math with ProcessSpeed
		    _charge -= MathF.Floor(_charge * 8f) / 256f * Scene.Instance.ProcessSpeed;
    		return true;
    	}
    	
	    _charge = Math.Min(_charge + 2f, 8f);

	    bool changePitch = AudioPlayer.Sound.IsPlaying(SoundStorage.Charge) && _charge > 0f;
    	_soundPitch = changePitch ? Math.Min(_soundPitch + 0.1f, 1.5f) : 1f;
    			
    	AudioPlayer.Sound.PlayPitched(SoundStorage.Charge, _soundPitch);
    	Data.Visual.OverrideFrame = 0;
    	
    	return true;
    }
}
