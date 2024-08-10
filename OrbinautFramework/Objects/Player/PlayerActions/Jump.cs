using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

public struct Jump(PlayerData data)
{
	public bool Perform()
	{
		if (!data.Physics.IsJumping || data.Physics.IsGrounded) return false;
		
		if (!data.Input.Down.Abc)
		{
			data.Physics.Velocity.MaxY(PhysicParams.MinimalJumpSpeed);
		}
		
		if (data.Physics.Velocity.Y < PhysicParams.MinimalJumpSpeed || CpuInputTimer == 0 && data.Id > 0) return false;

		if (Transform()) return true;
		
		switch (data.PlayerNode.Type)
		{
			case PlayerNode.Types.Sonic: JumpSonic(); break;
			case PlayerNode.Types.Tails: JumpTails(); break;
			case PlayerNode.Types.Knuckles: JumpKnuckles(); break;
			case PlayerNode.Types.Amy: JumpAmy(); break;
		}
		
		return false;
	}
	
	public bool Start()
	{
		if (data.State is States.SpinDash or States.Dash) return false;
		if (data.Physics.IsForcedSpin || !data.Physics.IsGrounded) return false;
		
		if (!data.Input.Press.Abc || !CheckCeilingDistance()) return false;
		
		if (!SharedData.FixJumpSize && SharedData.PhysicsType != PhysicsCore.Types.CD)
		{
			// Why do they even do that?
			data.Collision.Radius = data.Collision.RadiusNormal;
		}
	
		if (!data.Physics.IsSpinning)
		{
			data.PlayerNode.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
			data.Collision.Radius = data.Collision.RadiusSpin;
		}
		else if (!SharedData.NoRollLock && SharedData.PhysicsType != PhysicsCore.Types.CD)
		{
			data.Physics.IsAirLock = true;
		}
		
		float radians = Mathf.DegToRad(data.Rotation.Angle);
		data.Physics.Velocity.Vector += PhysicParams.JumpSpeed * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
		data.Physics.IsSpinning = true;
		data.Physics.IsJumping = true;
		data.Physics.IsGrounded = false;
		data.Collision.OnObject = null;
		data.Visual.SetPushBy = null;
		data.Collision.IsStickToConvex = false;
		data.Visual.Animation = Animations.Spin;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	
		// Exit control routine
		return true;
	}

	private bool CheckCeilingDistance()
	{
		const int maxCeilingDistance = 6; 
		
		data.TileCollider.SetData(
			(Vector2I)data.PlayerNode.Position,
			data.Collision.TileLayer,
			data.Collision.TileBehaviour);

		Vector2I radius = data.Collision.Radius;
		int distance = data.Collision.TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => data.TileCollider.FindClosestDistance(
				-radius.X, -radius.Y, radius.X, -radius.Y,
				true, Constants.Direction.Negative),
			
			Constants.TileBehaviours.RightWall => data.TileCollider.FindClosestDistance(
				-radius.Y, -radius.X, radius.Y, -radius.X,
				false, Constants.Direction.Negative),
			
			Constants.TileBehaviours.LeftWall => data.TileCollider.FindClosestDistance(
				-radius.Y, radius.X, radius.Y, radius.X,
				false, Constants.Direction.Positive),
			
			Constants.TileBehaviours.Ceiling => maxCeilingDistance,
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		return distance >= maxCeilingDistance;
	}

	private bool Transform()
	{
		if (!data.Input.Press.C || data.Super.IsSuper || SharedData.EmeraldCount != 7 || SharedData.PlayerRings < 50) return false;
		
		data.ResetState();
		AudioPlayer.Sound.Play(SoundStorage.Transform);
		AudioPlayer.Music.Play(MusicStorage.Super);
		//TODO: instance_create obj_star_super
		//instance_create(x, y, obj_star_super, { TargetPlayer: id });

		data.Physics.IsControlRoutineEnabled = false;
		data.Collision.IsObjectInteractionEnabled = false;			
		data.Damage.InvincibilityTimer = 0f;
		data.Super.Timer = 1f;
		data.State = States.Transform;
		data.Visual.Animation = Animations.Transform;
		ActionValue = SharedData.PhysicsType >= PhysicsCore.Types.S3 ? 26f : 36f;
		data.PlayerNode.Visible = true;
			
		// return player control routine
		return true;
	}

	private void JumpSonic()
	{
		if (SharedData.DropDash && data.State == States.None && !data.Input.Down.Abc)
		{
			if (data.PlayerNode.Shield.Type <= ShieldContainer.Types.Normal || 
			    data.Super.IsSuper || data.Item.InvincibilityTimer > 0f)
			{
				data.State = States.DropDash;
				ActionValue = 0f;
			}
		}
		
		// Barrier abilities
		if (!data.Input.Press.Abc || data.Super.IsSuper) return; 
		if (data.PlayerNode.Shield.State != ShieldContainer.States.None || data.Item.InvincibilityTimer > 0f) return;
		
		data.PlayerNode.Shield.State = ShieldContainer.States.Active;
		data.Physics.IsAirLock = false;
		
		switch (data.PlayerNode.Shield.Type)
		{
			case ShieldContainer.Types.None: JumpDoubleSpin(); break;
			case ShieldContainer.Types.Bubble: JumpWaterBarrier(); break;
			case ShieldContainer.Types.Fire: JumpFlameBarrier(); break;
			case ShieldContainer.Types.Lightning: JumpThunderBarrier(); break;
		}
	}

	private void JumpDoubleSpin()
	{
		if (!SharedData.DoubleSpin) return;
		
		//TODO: obj_double_spin
		/*
		with obj_double_spin
		{
			if TargetPlayer == other.id
			{
				instance_destroy();
			}
		}
		*/
		
		data.PlayerNode.Shield.State = ShieldContainer.States.DoubleSpin;
		
		//TODO: obj_double_spin
		//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.DoubleSpin);
	}

	private void JumpWaterBarrier()
	{
		data.Physics.Velocity.Vector = new Vector2(0f, 8f);
		//TODO: update shield animation
		data.PlayerNode.Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}
	
	private void JumpFlameBarrier()
	{
		data.SetCameraDelayX(16f);
				
		data.Physics.IsAirLock = true;
		data.Physics.Velocity.Vector = new Vector2(8f * (float)data.Visual.Facing, 0f);
		
		//TODO: update shield animation
		if (data.PlayerNode.Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			data.PlayerNode.Shield.Frame = 0;
		}
		else
		{
			data.PlayerNode.Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}
		
		//TODO: check data.PlayerNode.ZIndex
		data.PlayerNode.ZIndex = -1;
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldFire2);
	}

	private void JumpThunderBarrier()
	{
		data.PlayerNode.Shield.State = ShieldContainer.States.Disabled;
		data.Physics.Velocity.Y = -5.5f;
				
		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldLightning2);
	}

	private void JumpTails()
	{
		if (data.State != States.Default || !data.Input.Press.Abc) return;
		
		data.Physics.IsAirLock = false;
		data.Physics.IsSpinning = false;
		data.Physics.IsJumping = false;
		data.Physics.Gravity = GravityType.TailsDown;
		data.State = States.Flight;
		ActionValue = 480f;
		ActionValue2 = 0f;
		data.Collision.Radius = data.Collision.RadiusNormal;
				
		if (!data.Water.IsUnderwater)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}

		data.Input.Down = data.Input.Down with { Abc = false };
		data.Input.Press = data.Input.Press with { Abc = false };
	}

	private void JumpKnuckles()
	{
		if (data.State != States.Default || !data.Input.Press.Abc) return;
		
		data.Physics.IsAirLock = false;
		data.Physics.IsSpinning = false;
		data.Physics.IsJumping = false;	
		data.Visual.Animation = Animations.GlideAir;
		data.State = States.Glide;
		ActionState = (int)GlideStates.Air;
		ActionValue = data.Visual.Facing == Constants.Direction.Negative ? 0f : 180f;
		data.Collision.Radius = new Vector2I(10, 10);
		data.Physics.GroundSpeed.Value = 4f;
		data.Physics.Velocity.X = 0f;
		data.Physics.Velocity.Y += 2f; 
				
		if (data.Physics.Velocity.Y < 0f)
		{
			data.Physics.Velocity.Y = 0f;
		}
	}

	private void JumpAmy()
	{
		if (data.State != States.Default || !data.Input.Press.Abc) return;
		
		if (SharedData.NoRollLock)
		{
			data.Physics.IsAirLock = false;
		}
		
		data.Visual.Animation = Animations.HammerSpin;
		data.State = States.HammerSpin;
		ActionValue = 0f;
		
		AudioPlayer.Sound.Play(SoundStorage.Hammer);
	}
}