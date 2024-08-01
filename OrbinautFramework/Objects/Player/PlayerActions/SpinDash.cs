using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct SpinDash
{
    public Player Player { private get; init; }
    
    public bool Perform()
    {
	    if (!SharedData.SpinDash || !IsGrounded) return false;
    	
	    if (Start()) return false;
    	
	    // Continue if Spin Dash is being performed
	    if (Action != Actions.SpinDash) return false;
    	
	    if (Charge()) return false;
    	
	    SetCameraDelayX(16f);
    	
	    Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
	    Animation = Animations.Spin;
	    Action = Actions.None;
	    Radius = RadiusSpin;
	    IsSpinning = true;
    	
	    GroundSpeed.Value = ((IsSuper ? 11f : 8f) + MathF.Round(ActionValue) / 2f) * (float)Facing;
    	
	    AudioPlayer.Sound.Stop(SoundStorage.Charge);
	    AudioPlayer.Sound.Play(SoundStorage.Release);
    	
	    if (!SharedData.FixDashRelease) return true;
	    Velocity.SetDirectionalValue(GroundSpeed, Angle);
    	
	    return true;
    }

    private bool Start()
    {
    	if (Action != Actions.None || Animation is not (Animations.Duck or Animations.GlideLand)) return false;
    	if (!Input.Press.Abc || !Input.Down.Down) return true;
    	
    	Animation = Animations.SpinDash;
    	Action = Actions.SpinDash;
    	ActionValue = 0f;
    	ActionValue2 = 1f;
    	Velocity.Vector = Vector2.Zero;
    		
    	// TODO: SpinDash dust 
    	//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
    	AudioPlayer.Sound.Play(SoundStorage.Charge);
    	
    	return true;
    }

    private bool Charge()
    {
    	if (!Input.Down.Down) return false;
    	
    	if (!Input.Press.Abc)
    	{
    		//TODO: check math with ProcessSpeed
    		ActionValue -= MathF.Floor(ActionValue * 8f) / 256f * Scene.Local.ProcessSpeed;
    		return true;
    	}
    	
    	ActionValue = Math.Min(ActionValue + 2f, 8f);
    	
    	ActionValue2 = AudioPlayer.Sound.IsPlaying(SoundStorage.Charge) && ActionValue > 0f ? 
    		Math.Min(ActionValue2 + 0.1f, 1.5f) : 1f;
    			
    	AudioPlayer.Sound.PlayPitched(SoundStorage.Charge, ActionValue2);
    	OverrideAnimationFrame = 0;
    	
    	return true;
    }
}
