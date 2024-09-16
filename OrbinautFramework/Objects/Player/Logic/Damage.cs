using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;
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
	    
    	if (!logic.ControlType.IsCpu)
    	{
    		Scene.Instance.State = Scene.States.StopObjects;
    	}

	    IPlayerNode node = data.Node;
	    node.Shield.Type = ShieldContainer.Types.None;
	    data.Visual.ZIndex = (int)Constants.ZIndexes.AboveForeground;
	    data.Visual.Visible = true;
	    
	    MovementData movement = data.Movement;
	    movement.Gravity = GravityType.Default;
	    movement.Velocity = new Vector2(0f, -7f);
	    movement.GroundSpeed = 0f;
	    
	    logic.Action = States.Default;
    	data.Sprite.Animation = Animations.Death;
	    data.State = PlayerStates.Death;

    	if (data.Node.IsCameraTarget(out ICamera camera))
    	{
    		camera.IsMovementAllowed = false;
    	}
    }

    public void Hurt(float positionX) => Hurt(positionX, SoundStorage.Hurt);
    
    public void Hurt(float positionX, AudioStream sound)
    {
    	if (data.Damage.IsInvincible || data.State != PlayerStates.Control) return;

    	if (!logic.ControlType.IsCpu && SharedData.PlayerRings == 0 && 
	        data.Node.Shield.Type == ShieldContainer.Types.None)
    	{
    		Kill(sound);
    		return;
    	}
    	
    	logic.ResetData();
	    logic.Action = States.Default;
	    
	    data.State = PlayerStates.Hurt;
	    data.Sprite.Animation = Animations.Hurt;
	    data.Damage.InvincibilityTimer = 120f;
	    
    	const float velocityX = 2f, velocityY = 4f;
	    float velocity = (int)data.Movement.Position.X < (int)positionX ? -velocityX : velocityX;
	    
	    MovementData movement = data.Movement;
	    movement.Velocity = new Vector2(velocity, velocityY);
	    movement.GroundSpeed = 0f;
	    movement.Gravity = GravityType.HurtFall;
	    movement.IsAirLock = true;
	    
    	if (data.Water.IsUnderwater)
    	{
		    movement.Velocity = (Vector2)movement.Velocity * 0.5f;
		    movement.Gravity -= 0.15625f;
    	}
    	
    	if (data.Node.Shield.Type > ShieldContainer.Types.None)
    	{
		    data.Node.Shield.Type = ShieldContainer.Types.None;
    		AudioPlayer.Sound.Play(sound);
    		return;
    	}
	    
	    if (logic.ControlType.IsCpu) return;
    	
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

    public void Respawn()
    {
	    logic.Init();

	    if (data.Node.IsCameraTarget(out ICamera camera))
	    {
		    camera.IsMovementAllowed = true;
		    data.State = PlayerStates.Respawn;
		    return;
	    }

	    data.Movement.Position = new Vector2(byte.MaxValue, 0f);
	    data.Visual.ZIndex = (int)Constants.ZIndexes.AboveForeground; //TODO: RENDERER_DEPTH_HIGHEST
	    
	    data.Cpu.State = CpuLogic.States.RespawnInit;
	    data.State = PlayerStates.NoControl;
	    data.Movement.IsGrounded = false;
    }
}
