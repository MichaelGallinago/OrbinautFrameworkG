using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Dash(PlayerData data)
{
    public bool Perform()
    {
	    if (!SharedData.Dash || data.PlayerNode.Type != PlayerNode.Types.Sonic || 
	        data.Id > 0 && CpuInputTimer <= 0f) return false;
    	
	    StartDash();
    	
	    if (data.ActionType == Actions.Types.Dash && data.Physics.IsGrounded) return !ChargeDash() && ReleaseDash();
    	
	    if (data.ActionType != Actions.Types.Dash)
	    {
		    AudioPlayer.Sound.Stop(SoundStorage.Charge2);
	    }
	    return false;
    }

    private void StartDash()
    {
    	if (data.ActionType != Actions.Types.Default || data.Visual.Animation != Animations.LookUp) return;
	    if (!data.Input.Down.Up || !data.Input.Press.Abc) return;
    	
    	data.Visual.Animation = Animations.Move;
	    data.ActionType = Actions.Types.Dash;
    	ActionValue = 0f;
    	ActionValue2 = 0f;
    		
    	AudioPlayer.Sound.Play(SoundStorage.Charge2);
    }

    private bool ChargeDash()
    {
    	if (!Input.Down.Up) return false;
    	
    	if (ActionValue < 30f)
    	{
    		ActionValue += Scene.Instance.ProcessSpeed;
    	}

    	float acceleration = 0.390625f * (float)Facing * Scene.Instance.ProcessSpeed;
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