using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.PlayerActions;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public class PhysicalPlayerPhysicWithAbilities
{
	protected PhysicalPlayerPhysicWithAbilities()
	{
		LandHandler += ReleaseDropDash;
		LandHandler += ReleaseHammerSpin;
	}
	
	public void OnAttached(ICarrier carrier)
	{
		Vector2 previousPosition = carrier.CarryTargetPosition;
				
		if (Input.Press.Abc)
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 18f;
				
			IsSpinning = true;
			IsJumping = true;
			Action = new Default();
			Animation = Animations.Spin;
			Radius = RadiusSpin;
			Velocity.Vector = new Vector2(0f, PhysicParams.MinimalJumpSpeed);
					
			if (Input.Down.Left)
			{
				Velocity.X = -2f;
			}
			else if (Input.Down.Right)
			{
				Velocity.X = 2f;
			}
			
			AudioPlayer.Sound.Play(SoundStorage.Jump);
			return;
		}
		
		if (Action != Actions.Carried || carrier.Action != Actions.Flight || !Position.IsEqualApprox(previousPosition))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60f;
			Action = Actions.None;
			return;
		}
		
		AttachToPlayer(carrier);
	}
	
	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
		
		if (StartSpinDash()) return false;
		
		// Continue if Spin Dash is being performed
		if (Action != Actions.SpinDash) return false;
		
		if (ChargeSpinDash()) return false;
		
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

	private bool StartSpinDash()
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

	private bool ChargeSpinDash()
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
	
	private bool ProcessDash()
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

	private bool ProcessJump()
	{
		if (!IsJumping || IsGrounded) return false;
		
		if (!Input.Down.Abc)
		{
			Velocity.MaxY(PhysicParams.MinimalJumpSpeed);
		}
		
		if (Velocity.Y < PhysicParams.MinimalJumpSpeed || CpuInputTimer == 0 && Id > 0) return false;

		if (TransformInJump()) return true;
		
		switch (Type)
		{
			case Types.Sonic: JumpSonic(); break;
			case Types.Tails: JumpTails(); break;
			case Types.Knuckles: JumpKnuckles(); break;
			case Types.Amy: JumpAmy(); break;
		}
		
		return false;
	}

	private bool TransformInJump()
	{
		if (!Input.Press.C || IsSuper || SharedData.EmeraldCount != 7 || SharedData.PlayerRings < 50) return false;
		
		ResetState();
		AudioPlayer.Sound.Play(SoundStorage.Transform);
		AudioPlayer.Music.Play(MusicStorage.Super);
		//TODO: instance_create obj_star_super
		//instance_create(x, y, obj_star_super, { TargetPlayer: id });

		IsControlRoutineEnabled = false;
		IsObjectInteractionEnabled = false;			
		InvincibilityTimer = 0;
		SuperTimer = 1f;
		Action = Actions.Transform;
		Animation = Animations.Transform;
		ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26f : 36f;
		Visible = true;
			
		// return player control routine
		return true;
	}

	private void JumpSonic()
	{
		if (SharedData.DropDash && Action == Actions.None && !Input.Down.Abc)
		{
			if (Shield.Type <= ShieldContainer.Types.Normal || IsSuper || ItemInvincibilityTimer > 0f)
			{
				Action = Actions.DropDash;
				ActionValue = 0f;
			}
		}
		
		// Barrier abilities
		if (!Input.Press.Abc || IsSuper || 
		    Shield.State != ShieldContainer.States.None || ItemInvincibilityTimer != 0) return;
		
		Shield.State = ShieldContainer.States.Active;
		IsAirLock = false;
		
		switch (Shield.Type)
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
		
		Shield.State = ShieldContainer.States.DoubleSpin;
				
		//TODO: obj_double_spin
		//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
		AudioPlayer.Sound.Play(SoundStorage.DoubleSpin);
	}

	private void JumpWaterBarrier()
	{
		Velocity.Vector = new Vector2(0f, 8f);
		//TODO: update shield animation
		Shield.AnimationType = ShieldContainer.AnimationTypes.BubbleBounce;
		AudioPlayer.Sound.Play(SoundStorage.ShieldBubble2);
	}
	
	private void JumpFlameBarrier()
	{
		SetCameraDelayX(16f);
				
		IsAirLock = true;
		Velocity.Vector = new Vector2(8f * (float)Facing, 0f);
		
		//TODO: update shield animation
		if (Shield.AnimationType == ShieldContainer.AnimationTypes.FireDash)
		{
			Shield.Frame = 0;
		}
		else
		{
			Shield.AnimationType = ShieldContainer.AnimationTypes.FireDash;
		}
		
		//TODO: check ZIndex
		ZIndex = -1;
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldFire2);
	}

	private void JumpThunderBarrier()
	{
		Shield.State = ShieldContainer.States.Disabled;
		Velocity.Y = -5.5f;
				
		for (var i = 0; i < 4; i++)
		{
			//TODO: obj_barrier_sparkle
			//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
		}
		
		AudioPlayer.Sound.Play(SoundStorage.ShieldLightning2);
	}

	private void JumpTails()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;
		Gravity	= GravityType.TailsDown;
		Action = Actions.Flight;
		ActionValue = 480f;
		ActionValue2 = 0f;
		Radius = RadiusNormal;
				
		if (!IsUnderwater)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}

		Input.Down = Input.Down with { Abc = false };
		Input.Press = Input.Press with { Abc = false };
	}

	private void JumpKnuckles()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		IsAirLock = false;
		IsSpinning = false;
		IsJumping = false;	
		Animation = Animations.GlideAir;
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Air;
		ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
		Radius = new Vector2I(10, 10);
		GroundSpeed.Value = 4f;
		Velocity.X = 0f;
		Velocity.Y += 2f; 
				
		if (Velocity.Y < 0f)
		{
			Velocity.Y = 0f;
		}
	}

	private void JumpAmy()
	{
		if (Action != Actions.None || !Input.Press.Abc) return;
		
		if (SharedData.NoRollLock)
		{
			IsAirLock = false;
		}
		
		Animation = Animations.HammerSpin;
		Action = Actions.HammerSpin;
		ActionValue = 0f;
		
		AudioPlayer.Sound.Play(SoundStorage.Hammer);
	}

	private bool StartJump()
	{
		if (Action is Actions.SpinDash or Actions.Dash || IsForcedSpin || !IsGrounded) return false;
		
		if (!Input.Press.Abc || !CheckCeilingDistance()) return false;
		
		//TODO: check that Triangly fix "!global.player_physics != PHYSICS_CD"
		if (!SharedData.FixJumpSize && SharedData.PlayerPhysics != PhysicsTypes.CD)
		{
			// Why do they even do that?
			Radius = RadiusNormal;
		}
	
		if (!IsSpinning)
		{
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;
		}
		else if (!SharedData.NoRollLock && SharedData.PlayerPhysics != PhysicsTypes.CD)
		{
			IsAirLock = true;
		}
		
		float radians = Mathf.DegToRad(Angle);
		Velocity.Vector += PhysicParams.JumpSpeed * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
		IsSpinning = true;
		IsJumping = true;
		IsGrounded = false;
		OnObject = null;
		SetPushAnimationBy = null;
		IsStickToConvex = false;
		Animation = Animations.Spin;
		
		AudioPlayer.Sound.Play(SoundStorage.Jump);
	
		// Exit control routine
		return true;
	}

	private bool CheckCeilingDistance()
	{
		const int maxCeilingDistance = 6; 
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileBehaviour);
		
		int distance = TileBehaviour switch
		{
			Constants.TileBehaviours.Floor => TileCollider.FindClosestDistance(
				-Radius.X, -Radius.Y, Radius.X, -Radius.Y,
				true, Constants.Direction.Negative),
			
			Constants.TileBehaviours.RightWall => TileCollider.FindClosestDistance(
				-Radius.Y, -Radius.X, Radius.Y, -Radius.X,
				false, Constants.Direction.Negative),
			
			Constants.TileBehaviours.LeftWall => TileCollider.FindClosestDistance(
				-Radius.Y, Radius.X, Radius.Y, Radius.X,
				false, Constants.Direction.Positive),
			
			Constants.TileBehaviours.Ceiling => maxCeilingDistance,
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		return distance >= maxCeilingDistance;
	}

	private void ReleaseHammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
		if (ActionValue < MaxDropDashCharge) return;

		Animation = Animations.HammerDash;
		Action = Actions.HammerDash;
		ActionValue = 0f;
		GroundSpeed.Value = 6f * (float)Facing;
		
		if (IsSuper && IsCameraTarget(out ICamera camera))
		{
			camera.SetShakeTimer(6f);
		}
		
		AudioPlayer.Sound.Stop(SoundStorage.Charge3);
		AudioPlayer.Sound.Play(SoundStorage.Release);
	}
}
