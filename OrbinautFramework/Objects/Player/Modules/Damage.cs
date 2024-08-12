using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Damage(PlayerData data)
{
    public void Kill()
    {
    	if (data.Death.IsDead) return;
    	
	    data.ResetState();
    	AudioPlayer.Sound.Play(SoundStorage.Hurt);

    	if (data.Id == 0)
    	{
    		Scene.Instance.State = Scene.States.StopObjects;
    		
    		SharedData.PlayerShield = ShieldContainer.Types.None;
    	}
    	
    	data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground;
	    data.Node.Visible = true;
	    
    	data.State = States.Default;
    	data.Visual.Animation = Animations.Death;
    	data.Death.IsDead = true;
    	data.Collision.IsObjectInteractionEnabled = false;
    	data.Movement.Gravity = GravityType.Default;
    	data.Movement.Velocity.Vector = new Vector2(0f, -7f);
    	data.Movement.GroundSpeed.Value = 0f;

    	if (data.IsCameraTarget(out ICamera camera))
    	{
    		camera.IsMovementAllowed = false;
    	}
    }
    
    public void Hurt(float positionX = 0f)
    {
    	if (data.Damage.IsInvincible || IsDebugMode) return;

    	if (data.Id == 0 && SharedData.PlayerRings == 0 && SharedData.PlayerShield == ShieldContainer.Types.None)
    	{
    		Kill();
    		return;
    	}
    	
    	data.ResetState();

    	const float velocityX = 2f, velocityY = 4f;
    	data.Movement.Velocity.Vector = 
		    new Vector2(data.Node.Position.X - positionX < 0f ? -velocityX : velocityX, velocityY);
	    
    	data.Movement.Gravity = GravityType.HurtFall;
    	data.Visual.Animation = Animations.Hurt;
    	data.Damage.IsHurt = true;
    	data.Movement.IsAirLock = true;
    	data.Damage.InvincibilityTimer = 120f;

    	if (data.Water.IsUnderwater)
    	{
    		data.Movement.Velocity.Vector *= 0.5f;
    		data.Movement.Gravity -= 0.15625f;
    	}
    	
    	if (data.Id > 0 || SharedData.PlayerShield > ShieldContainer.Types.None)
    	{
    		if (data.Id == 0)
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