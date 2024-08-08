using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Damage
{
    public void Kill()
    {
    	if (IsDead) return;
    	
    	ResetState();
    	AudioPlayer.Sound.Play(SoundStorage.Hurt);

    	if (Id == 0)
    	{
    		Scene.Instance.State = Scene.States.StopObjects;
    		
    		SharedData.PlayerShield = ShieldContainer.Types.None;
    	}
    	
    	ZIndex = (int)Constants.ZIndexes.AboveForeground;
	    Visible = true;
	    
    	Action = Actions.None;
    	Animation = Animations.Death;
    	IsDead = true;
    	IsObjectInteractionEnabled = false;
    	Gravity = GravityType.Default;
    	Velocity.Vector = new Vector2(0f, -7f);
    	GroundSpeed.Value = 0f;

    	if (IsCameraTarget(out ICamera camera))
    	{
    		camera.IsMovementAllowed = false;
    	}
    }
    
    public void Hurt(float positionX = 0f)
    {
    	if (IsInvincible || IsDebugMode) return;

    	if (Id == 0 && SharedData.PlayerRings == 0 && SharedData.PlayerShield == ShieldContainer.Types.None)
    	{
    		Kill();
    		return;
    	}
    	
    	ResetState();

    	const float velocityX = 2f, velocityY = 4f;
    	Velocity.Vector = new Vector2(Position.X - positionX < 0f ? -velocityX : velocityX, velocityY);
    	Gravity = GravityType.HurtFall;
    	Animation = Animations.Hurt;
    	IsHurt = true;
    	IsAirLock = true;
    	InvincibilityTimer = 120f;

    	if (IsUnderwater)
    	{
    		Velocity.Vector *= 0.5f;
    		Gravity -= 0.15625f;
    	}
    	
    	if (Id > 0 || SharedData.PlayerShield > ShieldContainer.Types.None)
    	{
    		if (Id == 0)
    		{
    			SharedData.PlayerShield = ShieldContainer.Types.None;
    		}
    		
    		AudioPlayer.Sound.Play(SoundStorage.Hurt);
    		return;
    	}
    	
    	DropRings();
    }
    
    private void DropRings()
    {
	    int ringFlip = -1;
	    var ringAngle = 101.25f;
	    var ringSpeed = 4;
	    uint count = Math.Min(SharedData.PlayerRings, 32);
				
	    for (var i = 0; i < count; i++) 
	    {
		    //TODO: spawn ring
		    /*
		    instance_create(_player.x, _player.y, obj_ring,
		    {
			    State: RING_STATE_DROPPED,
			    VelocityX: ringSpeed * dcos(ringAngle) * -ringFlip,
			    VelocityY: ringSpeed * -dsin(ringAngle)
		    });
		    */
							
		    if (ringFlip == 1)
		    {
			    ringAngle += 22.5f;
		    }
							
		    ringFlip = -ringFlip;

		    if (i != 15) continue;
			
		    ringSpeed = 2;
		    ringAngle = 101.25f;
	    }

	    Scene.Instance.RingSpillTimer = 256f;
	
	    SharedData.PlayerRings = 0;
	    SharedData.LifeRewards = SharedData.LifeRewards with { X = 100 };
			
	    AudioPlayer.Sound.Play(SoundStorage.RingLoss);
    }
}