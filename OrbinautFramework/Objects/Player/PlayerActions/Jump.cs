using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.PlayerActions;

[FsmSourceGenerator.FsmState("Action")]
public struct Jump(PlayerData data)
{
	public void Enter() //TODO: move part of the code if not all of it is needed
	{
		if (!SharedData.FixJumpSize && SharedData.PhysicsType != PhysicsCore.Types.CD)
		{
			// Why do they even do that?
			data.Collision.Radius = data.Collision.RadiusNormal;
		}
	
		if (!data.Movement.IsSpinning)
		{
			data.Node.Position += new Vector2(0f, data.Collision.Radius.Y - data.Collision.RadiusSpin.Y);
			data.Collision.Radius = data.Collision.RadiusSpin;
		}
		else if (!SharedData.NoRollLock && SharedData.PhysicsType != PhysicsCore.Types.CD)
		{
			data.Movement.IsAirLock = true;
		}
		
		float radians = Mathf.DegToRad(data.Movement.Angle);
		var velocity = new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		data.Movement.Velocity.Vector += data.Physics.JumpSpeed * velocity;
		
		data.Movement.IsSpinning = true;
		data.Movement.IsGrounded = false;
		
		data.Collision.OnObject = null;
		data.Collision.IsStickToConvex = false;
		
		data.Visual.SetPushBy = null;
		data.Visual.Animation = Animations.Spin;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	}
	
	public bool Perform()
	{
		if (data.Movement.IsGrounded) return false;
		
		if (!data.Input.Down.Abc)
		{
			data.Movement.Velocity.MaxY(data.Physics.MinimalJumpSpeed);
		}
		
		if (data.Movement.Velocity.Y < data.Physics.MinimalJumpSpeed) return false; 
		if (CpuInputTimer == 0 && data.Id > 0) return false;
		
		if (Transform()) return true;
		
		switch (data.Node.Type)
		{
			case PlayerNode.Types.Sonic: JumpSonic(); break;
			case PlayerNode.Types.Tails: JumpTails(); break;
			case PlayerNode.Types.Knuckles: JumpKnuckles(); break;
			case PlayerNode.Types.Amy: JumpAmy(); break;
		}
		
		return false;
	}

	private bool Transform()
	{
		if (!data.Input.Press.C || data.Super.IsSuper) return false;
		if (SharedData.EmeraldCount != 7 || SharedData.PlayerRings < 50) return false;
		
		data.ResetState();
		data.State = States.Transform;
		
		// return player control routine
		return true;
	}

	private void JumpSonic()
	{
		if (SharedData.DropDash && data.State == States.None && !data.Input.Down.Abc)
		{
			if (data.Node.Shield.Type <= ShieldContainer.Types.Normal || 
			    data.Super.IsSuper || data.Item.InvincibilityTimer > 0f)
			{
				data.State = States.DropDash;
			}
		}
		
		// Barrier abilities
		if (!data.Input.Press.Abc || data.Super.IsSuper) return; 
		if (data.Node.Shield.State != ShieldContainer.States.None || data.Item.InvincibilityTimer > 0f) return;
		
		data.Node.Shield.State = ShieldContainer.States.Active;
		data.Movement.IsAirLock = false;
		
		switch (data.Node.Shield.Type)
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
		
		data.Node.Shield.State = ShieldContainer.States.DoubleSpin;
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
		
		//TODO: obj_double_spin
		//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.DoubleSpin);
	}

	private void JumpWaterBarrier()
	{
		data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
		data.Movement.Velocity.Vector = new Vector2(0f, 8f);
		//TODO: update shield animation
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}
	
	private void JumpFlameBarrier()
	{
		data.SetCameraDelayX(16f);
				
		data.Movement.IsAirLock = true;
		data.Movement.Velocity.Vector = new Vector2(8f * (float)data.Visual.Facing, 0f);
		
		//TODO: update shield animation
		if (data.Node.Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			data.Node.Shield.Frame = 0;
		}
		else
		{
			data.Node.Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}
		
		//TODO: check data.PlayerNode.ZIndex
		data.Node.ZIndex = -1;
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldFire2);
	}

	private void JumpThunderBarrier()
	{
		data.Node.Shield.State = ShieldContainer.States.Disabled;
		data.Movement.Velocity.Y = -5.5f;
				
		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldLightning2);
	}

	private void JumpTails()
	{
		if (!data.Input.Press.Abc) return;
		
		data.State = States.Flight;
	}

	private void JumpKnuckles()
	{
		if (!data.Input.Press.Abc) return;
		
		data.State = States.GlideAir;
	}

	private void JumpAmy()
	{
		if (!data.Input.Press.Abc) return;
		
		data.State = States.HammerSpin;
	}
}