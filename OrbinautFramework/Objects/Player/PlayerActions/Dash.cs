using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Dash
{
    public Player Player { private get; init; }
    
    public bool Perform()
    {
	    if (!SharedData.Dash || Type != Types.Sonic || Id > 0 && CpuInputTimer <= 0f) return false;
    	
	    StartDash();
    	
	    if (Action == Actions.Dash && IsGrounded) return !ChargeDash() && ReleaseDash();
    	
	    if (Action != Actions.Dash)
	    {
		    AudioPlayer.Sound.Stop(SoundStorage.Charge2);
	    }
	    return false;
    }

    private void StartDash()
    {
    	if (Action != Actions.None || Animation != Animations.LookUp || !Input.Down.Up || !Input.Press.Abc) return;
    	
    	Animation = Animations.Move;
    	Action = Actions.Dash;
    	ActionValue = 0f;
    	ActionValue2 = 0f;
    		
    	AudioPlayer.Sound.Play(SoundStorage.Charge2);
    }

    private bool ChargeDash()
    {
    	if (!Input.Down.Up) return false;
    	
    	if (ActionValue < 30f)
    	{
    		ActionValue += Scene.Local.ProcessSpeed;
    	}

    	float acceleration = 0.390625f * (float)Facing * Scene.Local.ProcessSpeed;
    	float launchSpeed = PhysicParams.AccelerationTop * (ItemSpeedTimer > 0f || IsSuper ? 1.5f : 2f);
    	ActionValue2 = Math.Clamp(ActionValue2 + acceleration, -launchSpeed, launchSpeed);
    	GroundSpeed.Value = ActionValue2;
    	return true;
    }

    private bool ReleaseDash()
    {
    	AudioPlayer.Sound.Stop(SoundStorage.Charge2);
    	Action = Actions.None;
    	
    	if (ActionValue < 30f)
    	{
    		GroundSpeed.Value = 0f;
    		return false;
    	}

    	SetCameraDelayX(16f);
    	
    	AudioPlayer.Sound.Play(SoundStorage.Release2);	
    	
    	if (!SharedData.FixDashRelease) return true;
    	Velocity.SetDirectionalValue(GroundSpeed, Angle);
    	return true;
    }
}