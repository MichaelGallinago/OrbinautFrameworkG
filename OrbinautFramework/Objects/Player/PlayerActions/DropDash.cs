using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct DropDash : IAction
{
    public void Perform(Player player)
    {
        
    }
    
    private void ChargeDropDash()
    {
	    if (IsGrounded || CancelDropDash()) return;
		
	    if (Input.Down.Abc)
	    {
		    IsAirLock = false;		
		    ActionValue += Scene.Local.ProcessSpeed;
			
		    if (ActionValue < MaxDropDashCharge || Animation == Animations.DropDash) return;
			
		    AudioPlayer.Sound.Play(SoundStorage.Charge3);
		    Animation = Animations.DropDash;
		    return;
	    }
		
	    switch (ActionValue)
	    {
		    case <= 0f:
			    return;
			
		    case >= MaxDropDashCharge:
			    Animation = Animations.Spin;
			    Action = Actions.DropDashCancel;
			    break;
	    }
		
	    ActionValue = 0f;
    }
    
    private void ReleaseDropDash()
    {
    	if (CancelDropDash()) return;
    	
    	if (ActionValue < MaxDropDashCharge) return;
    	
    	Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
    	Radius = RadiusSpin;
    	
    	if (IsSuper)
    	{
    		UpdateDropDashGroundSpeed(13f, 12f);
    		if (IsCameraTarget(out ICamera camera))
    		{
    			camera.SetShakeTimer(6f);
    		}
    	}
    	else
    	{
    		UpdateDropDashGroundSpeed(12f, 8f);
    	}
    	
    	Animation = Animations.Spin;
    	IsSpinning = true;
    	
    	SetCameraDelayX(8f);
    		
    	//TODO: obj_dust_dropdash
    	//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
    	AudioPlayer.Sound.Stop(SoundStorage.Charge3);
    	AudioPlayer.Sound.Play(SoundStorage.Release);
    }

    private bool CancelDropDash()
    {
    	if (!SharedData.DropDash || Action != Actions.DropDash) return true;
    	
    	if (Shield.Type <= ShieldContainer.Types.Normal || IsSuper || ItemInvincibilityTimer > 0f) return false;
    	
    	Animation = Animations.Spin;
    	Action = Actions.None;
    	return true;
    }

    private void UpdateDropDashGroundSpeed(float limitSpeed, float force)
    {
    	var sign = (float)Facing;
    	limitSpeed *= sign;
    	force *= sign;
    	
    	if (Velocity.X * sign >= 0f)
    	{
    		GroundSpeed.Value = MathF.Floor(GroundSpeed / 4f) + force;
    		if (sign * GroundSpeed <= limitSpeed) return;
    		GroundSpeed.Value = limitSpeed;
    		return;
    	}
    	
    	GroundSpeed.Value = force;
    	if (Mathf.IsZeroApprox(Angle)) return;
    	
    	GroundSpeed.Value += MathF.Floor(GroundSpeed / 2f);
    }
}