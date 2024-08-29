using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct Damage(PlayerData data, IPlayerLogic logic)
{
	public void Kill() => Kill(SoundStorage.Hurt);
	
    public void Kill(AudioStream sound)
    {
    	if (data.State == PlayerStates.Death) return;
    	
	    logic.ResetData();
    	AudioPlayer.Sound.Play(sound);
	    
    	if (data.Id == 0)
    	{
    		Scene.Instance.State = Scene.States.StopObjects;
    		SharedData.PlayerShield = ShieldContainer.Types.None;
    	}
    	
    	data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground;
	    data.Node.Visible = true;
	    
	    logic.Action = States.Default;
    	data.Sprite.Animation = Animations.Death;
	    data.State = PlayerStates.Death;
    	data.Movement.Gravity = GravityType.Default;
    	data.Movement.Velocity.Vector = new Vector2(0f, -7f);
    	data.Movement.GroundSpeed.Value = 0f;

    	if (data.IsCameraTarget(out ICamera camera))
    	{
    		camera.IsMovementAllowed = false;
    	}
    }

    public void Hurt(float positionX) => Hurt(positionX, SoundStorage.Hurt);
    
    public void Hurt(float positionX, AudioStream sound)
    {
    	if (data.Damage.IsInvincible || data.State != PlayerStates.Control) return;

    	if (data.Id == 0 && SharedData.PlayerRings == 0 && SharedData.PlayerShield == ShieldContainer.Types.None)
    	{
    		Kill(sound);
    		return;
    	}
    	
    	logic.ResetData();
	    logic.Action = States.Default;
	    data.State = PlayerStates.Hurt;

    	const float velocityX = 2f, velocityY = 4f;
	    float velocity = data.Node.Position.X - positionX < 0f ? -velocityX : velocityX;
    	data.Movement.Velocity.Vector = new Vector2(velocity, velocityY);
	    
    	data.Movement.Gravity = GravityType.HurtFall;
    	data.Sprite.Animation = Animations.Hurt;
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
    		
    		AudioPlayer.Sound.Play(sound);
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

    public void Respawn() //TODO: remove repeat in CPU
    {
	    logic.Init();
	    
	    if (!logic.ControlType.IsCpu)
	    {
		    if (data.IsCameraTarget(out ICamera camera))
		    {
			    camera.IsMovementAllowed = true;
		    }
		    data.Damage.InvincibilityTimer = 60f;
		    return;
	    }

	    data.Node.Position = new Vector2(byte.MaxValue, 0f);
	    data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground; //TODO: RENDERER_DEPTH_HIGHEST
	    data.Cpu.State = CpuLogic.States.RespawnInit;
	    data.Movement.IsGrounded = false;
    }
}
