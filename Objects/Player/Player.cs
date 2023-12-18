using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using OrbinautFramework3.Objects.Spawnable.PlayerParticles;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : Framework.CommonObject.CommonObject
{
	#region Constants

	private const byte EditModeAccelerationMultiplier = 4;
	private const float EditModeAcceleration = 0.046875f;
	private const byte EditModeSpeedLimit = 16;
	private const byte MaxDropDashCharge = 20;
	private const byte DefaultViewTime = 120;
	
	public const byte CpuDelay = 16;
    
	// TODO: PlayerCount
	// byte PlayerCount = instance_number(SharedData.player_obj)
	public enum Types : byte
	{
		None, Sonic, Tails, Knuckles, Amy, Global, GlobalAI
	}
    
	public enum States : byte
	{
		Normal,
		Spin,
		Jump,
		SpinDash,
		PeelOut,
		DropDash,
		Flight,
		HammerRush,
		HammerSpin,
		Carried
	}

	public enum CpuStates : byte
	{
		Main, Fly, Stuck
	}
    
	public enum PhysicsTypes : byte
	{
		S1, CD, S2, S3, SK
	}
    
	public enum Actions : byte
	{
		None,
		SpinDash,
		PeelOut,
		DropDash,
		DropDashCancel,
		Glide,
		GlideCancel,
		Climb,
		Flight,
		TwinAttack,
		TwinAttackCancel,
		Transform,
		HammerRush,
		HammerSpin,
		HammerSpinCancel,
		Carried,
		ObjectControl
	}

	public enum GlideStates : byte
	{
		Air, Ground, Fall
	}
    
	public enum Animations : byte
	{
		Idle,
		Move,
		Spin,
		DropDash,
		SpinDash,
		Push,
		Duck,
		LookUp,
		Fly,
		FlyTired,
		Swim,
		SwimTired,
		Hurt,
		Death,
		Drown,
		GlideAir,
		GlideFall,
		GlideGround,
		GlideLand,
		ClimbWall,
		ClimbLedge,
		Skid,
		Balance,
		BalanceFlip,
		BalancePanic,
		BalanceTurn,
		Bounce,
		Transform,
		Breathe,
		HammerSpin,
		HammerRush,
		FlyLift,
		SwimLift,
		Grab
	}

	public enum RestartStates : byte
	{
		GameOver, ResetLevel, RestartStage, RestartGame
	}
	
	#endregion

	#region Variables

	public static List<Player> Players { get; }
    
	[Export] public Types Type;

	public int Id { get; private set; }
	
	public PhysicParams PhysicParams { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public Vector2 Speed;
	public float GroundSpeed { get; set; }
	public float Angle { get; set; }
	public float SlopeGravity { get; set; }

	public Constants.TileLayer TileLayer { get; set; }
	public Constants.GroundMode GroundMode { get; set; }
	public bool StickToConvex { get; set; }
    
	public bool ObjectInteraction { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public Framework.CommonObject.CommonObject PushingObject { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public Framework.CommonObject.CommonObject OnObject { get; set; }
	public bool IsSuper { get; set; }
	public bool IsInvincible { get; set; }
	public int SuperValue { get; set; }

	public Actions Action { get; set; }
	public int ActionState { get; set; }
	public float ActionValue { get; set; }
	public float ActionValue2 { get; set; }
	public bool BarrierFlag { get; set; }
	public Barrier Barrier { get; set; }
    
	public Constants.Direction Facing { get; set; }
	public Animations Animation { get; set; }
	public float AnimationTimer { get; set; }
	public float VisualAngle { get; set; }
    
	public int CameraViewTimer { get; set; }
    
	public bool IsForcedRoll { get; set; }
	public float GroundLockTimer { get; set; }
	public bool IsAirLock { get; set; }
    
	public float AirTimer { get; set; }
	public uint ComboCounter { get; set; }
	public uint ScoreCount { get; set; }
	public uint RingCount { get; set; }
	public uint LifeCount { get; set; }
	public float InvincibilityFrames { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }
	public uint[] LifeRewards { get; set; }

	public Player CarryTarget { get; set; }
	public float CarryTimer { get; set; }
	public Vector2 CarryParentPosition { get; set; }
    
	public CpuStates CpuState { get; set; }
	public float CpuTimer { get; set; }
	public float CpuInputTimer { get; set; }
	public bool IsCpuJumping { get; set; }
	public bool IsCpuRespawn { get; set; }
	public Player CpuTarget { get; set; }
    
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }

	public Buttons InputPress { get; set; }
	public Buttons InputDown { get; set; }

	public List<RecordedData> RecordedData { get; set; }

	// Edit mode
	public bool IsEditMode { get; private set; }
	public int EditModeIndex { get; private set; }
	public float EditModeSpeed { get; private set; }
	public List<Type> EditModeObjects { get; private set; }
	
	#endregion
    
	static Player()
	{
		Players = [];
	}

	public override void _Ready()
	{
		SetBehaviour(BehaviourType.Unique);
        
		switch (Type)
		{
			case Types.Tails:
				RadiusNormal = new Vector2I(9, 15);
				RadiusSpin = new Vector2I(7, 14);
				break;
			case Types.Amy:
				RadiusNormal = new Vector2I(9, 16);
				RadiusSpin = new Vector2I(7, 12);
				break;
			default:
				RadiusNormal = new Vector2I(9, 19);
				RadiusSpin = new Vector2I(7, 14);
				break;
		}

		Radius = RadiusNormal;

		Position = new Vector2(Position.X, Position.Y - Radius.Y + 1);

		Gravity = GravityType.Default;
		TileLayer = Constants.TileLayer.Main;
		GroundMode = Constants.GroundMode.Floor;
		ObjectInteraction = true;
		Barrier = new Barrier(this);
		Facing = Constants.Direction.Positive;
		Animation = Animations.Idle;
		AirTimer = 1800f;
		CpuState = CpuStates.Main;
		RestartState = RestartStates.GameOver;
		InputPress = new Buttons();
		InputDown = new Buttons();
		CameraViewTimer = DefaultViewTime;
		RecordedData = [];

		if (Type == Types.Tails)
		{
			var tail = new Tail(this);
			AddChild(tail);
		}
		
		if (FrameworkData.GiantRingData != null)
		{
			Position = (Vector2)FrameworkData.GiantRingData;
		}
		else if (Id == 0)
		{
			// TODO: Respawn CPU on the checkpoint
			
			if (FrameworkData.CheckpointData != null)
			{
				Vector2I position = FrameworkData.CheckpointData.Position;
				position.Y -= Radius.Y;
				Position = position;
			}
			
			if (FrameworkData.PlayerBackupData != null)
			{
				RingCount = FrameworkData.PlayerBackupData.RingCount;
				Barrier.Type = FrameworkData.PlayerBackupData.BarrierType;
			}
		}
		
		if (Id == 0)
		{
			if (FrameworkData.SavedBarrier != Barrier.Types.None)
			{
				Barrier.Type = FrameworkData.SavedBarrier;
			}
		
			FrameworkData.SavedBarrier = 0;
			FrameworkData.SavedRings = 0;
		}
		
		ScoreCount = FrameworkData.SavedScore;
		RingCount = FrameworkData.SavedRings;
		LifeCount = FrameworkData.SavedLives;
		
		LifeRewards = new[] { RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000 };
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Id = Players.Count;
		Players.Add(this);
		FrameworkData.CurrentScene.AddPlayerStep(this);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Players.Remove(this);
		for (int i = Id; i < Players.Count; i++)
		{
			Players[i].Id--;
		}
		FrameworkData.CurrentScene.RemovePlayerStep(this);
		if (Players.Count == 0 || !IsCpuRespawn) return;
		var newPlayer = new Player
		{
			Type = Type,
			Position = Players.First().Position
		};

		newPlayer.PlayerStep(FrameworkData.ProcessSpeed);
	}

	public void PlayerStep(double processSpeed)
	{
		if (FrameworkData.IsPaused || !FrameworkData.UpdateObjects && !IsDead) return;
		
		var processSpeedF = (float)processSpeed; 
		
		// Process local input
		UpdateInput();

		// Process Edit Mode
		if (ProcessEditMode(processSpeedF)) return;
	    
		// Process CPU Player logic (return if flying in or respawning)
		if (ProcessAI(processSpeedF)) return;
	    
		// Process Restart Event
		ProcessRestart(processSpeedF);
	    
		// Process default player control routine
		UpdatePhysics();
		
		ProcessCamera();
		UpdateStatus();
		ProcessWater();
		UpdateCollision();
		RecordData();
		ProcessRotation();
		ProcessAnimate();
		ProcessPalette();
	}
	
	#region UpdatePhysics
	
	private void UpdatePhysics()
	{
		if (Action is Actions.ObjectControl or Actions.Transform) return;

		// Define physics for this step
		PhysicParams = PhysicParams.Get(IsUnderwater, IsSuper, Type, ItemSpeedTimer);
		
		// Abilities logic
		if (ProcessSpinDash()) return;
		if (ProcessPeelOut()) return;
		if (ProcessJump()) return;
		if (StartJump()) return;
		
		ProcessDropDash();
		ProcessFlight();
		ProcessClimb();
		ProcessGlide();
		ProcessHammerSpin();
		ProcessHammerRush();
		
		// Core player logic
		ProcessSlopeResist();
		ProcessSlopeResistRoll();
		ProcessMovementGround();
		ProcessMovementGroundRoll();
		ProcessMovementAir();
		ProcessBalance();
		ProcessCollisionGroundWalls();
		ProcessRollStart();
		ProcessLevelBound();
		ProcessPosition();
		ProcessCollisionGroundFloor();
		ProcessSlopeRepel();
		ProcessCollisionAir();
		
		// Late abilities logic
		ProcessGlideCollision();
		ProcessCarry();
	}

	private bool ProcessSpinDash()
	{
		if (!SharedData.SpinDash || !IsGrounded) return false;
	
		// Start Spin Dash (initial charge)
		if (Action == Actions.None && Animation is Animations.Duck or Animations.GlideLand)
		{
			if (!InputPress.Abc || !InputDown.Down) return false;
			
			Animation = Animations.SpinDash;
			Action = Actions.SpinDash;
			ActionValue = 0;
			ActionValue2 = 1;
			Speed = new Vector2();
			
			// TODO: audio & SpinDash dust 
			//instance_create(x, y + Radius.Y, obj_dust_spindash, { TargetPlayer: id });
			//audio_play_sfx(sfx_charge);
		
			// Register next charge next frame
			return false;
		}
	
		// Continue if Spin Dash is being performed
		if (Action != Actions.SpinDash) return false;
	
		if (InputDown.Down)
		{
			if (InputPress.Abc)
			{
				ActionValue = Math.Min(ActionValue + 2, 8);
				
				// TODO: audio
				/*
				ActionValue2 = audio_is_playing(sfx_charge) && ActionValue > 0
					? Math.Min(ActionValue2 + 0.1f, 1.5f)
					: 1;
				
				
				var sound = audio_play_sfx(sfx_charge);
				audio_sound_pitch(sound, ActionValue2);
				*/
				Sprite.UpdateFrame(0);
			}
			else
			{
				ActionValue -= MathF.Floor(ActionValue / 0.125f) / 256f;
			}
		}
		else
		{
			if (!SharedData.CDCamera && Id == 0)
			{
				Camera.MainCamera.Delay.X = 16;
			}
		
			int baseSpeed = IsSuper ? 11 : 8;
		
			Position = new Vector2(Position.X, Position.Y + Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;
			Animation = Animations.Spin;
			IsSpinning = true;
			Action = Actions.None;
			GroundSpeed = (baseSpeed + MathF.Round(ActionValue) / 2) * (float)Facing;
				
			if (SharedData.FixDashRelease)
			{
				float radians = Mathf.DegToRad(Angle);
				Speed = GroundSpeed * new Vector2(MathF.Cos(radians), -MathF.Sin(radians));
			}
			
			//TODO: audio
			//audio_stop_sfx(sfx_charge);
			//audio_play_sfx(sfx_release);
			
			// return player control routine
			return true;
		}
		
		return false;
	}
	
	private bool ProcessPeelOut()
	{
		if (!SharedData.PeelOut || Type != Types.Sonic || Id > 0 || !IsGrounded) return false;
	
		// Start Super Peel Out
		if (Action == Actions.None && Animation == Animations.LookUp && InputDown.Up && InputPress.Abc)
		{
			Animation = Animations.Move;
			Action = Actions.PeelOut;
			ActionValue = 0;
			ActionValue2 = 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_charge2, [1.00, 2.30]);
		}
	
		// Continue if Super Peel Out is being performed
		if (Action != Actions.PeelOut) return false;
	
		float launchSpeed = PhysicParams.AccelerationTop * (ItemSpeedTimer > 0f || IsSuper ? 1.5f : 2f);
		if (InputDown.Up)
		{
			if (ActionValue < 30f)
			{
				ActionValue++;
			}
			
			ActionValue2 = Math.Clamp(ActionValue2 + 0.390625f * (float)Facing, -launchSpeed, launchSpeed);
			GroundSpeed = ActionValue2;
		}
		else 
		{
			//TODO: audio
			//audio_stop_sfx(sfx_charge2);
			Action = Actions.None;
		
			if (ActionValue != 30f)
			{
				GroundSpeed = 0f;
			}
			else
			{
				if (!SharedData.CDCamera && Id == 0)
				{
					Camera.MainCamera.Delay.X = 16;
				}
			
				if (SharedData.FixDashRelease)
				{
					float radians = Mathf.DegToRad(Angle);
					Speed = GroundSpeed * new Vector2(MathF.Cos(radians), -MathF.Sin(radians));
				}
			
				//TODO: audio
				//audio_play_sfx(sfx_release2);
			
				// return player control routine
				return true;
			}
		}

		return false;
	}

	private bool ProcessJump()
	{
		if (!IsJumping || IsGrounded) return false;
		
		if (Type == Types.Amy && Action == Actions.None && Speed.Y >= 0)
		{
			Animation = Animations.Spin;
			SetHitboxExtra(new Vector2I(0, 0));
		}
		
		if (!InputDown.Abc)
		{
			Speed.Y = Math.Max(Speed.Y, PhysicParams.MinimalJumpVelocity);
		}
		
		if (Speed.Y < PhysicParams.MinimalJumpVelocity || Id > 0 && CpuInputTimer == 0) return false;
		
		if (InputPress.C && SharedData.EmeraldCount == 7 && !IsSuper && RingCount >= 50)
		{
			ResetState();
			//TODO: audio
			//audio_play_sfx(sfx_transform);
			//audio_play_bgm(bgm_super);
			//TODO: instance_create obj_star_super
			//instance_create(x, y, obj_star_super, { TargetPlayer: id });
				
			ObjectInteraction = false;			
			InvincibilityFrames = 0;
			IsSuper = true;
			Action = Actions.Transform;
			ActionValue = SharedData.PlayerPhysics >= PhysicsTypes.S3 ? 26 : 36;
			Animation = Animations.Transform;
			AnimationTimer = Type == Types.Sonic ? 39 : 36;
			
			// return player control routine
			return true;
		}
		
		switch (Type)
		{
			case Types.Sonic:
				if (SharedData.DropDash && Action == Actions.None && !InputDown.Abc)
				{
					if (Barrier.Type <= Barrier.Types.Normal || IsSuper)
					{
						Action = Actions.DropDash;
						ActionValue = 0;
					}
				}
				
				// Barrier abilities
				if (!InputPress.Abc || IsSuper || Barrier.State != Barrier.States.None || ItemInvincibilityTimer != 0) break;
				
				Barrier.State = Barrier.States.Active;
				IsAirLock = false;
				
				switch (Barrier.Type)
				{
					case Barrier.Types.None:
						if (!SharedData.DoubleSpin) break;
						
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
						
						Barrier.State = Barrier.States.DoubleSpin;
						
						//TODO: audio & obj_double_spin
						//instance_create(x, y, obj_double_spin, { TargetPlayer: id });
						//audio_play_sfx(sfx_double_spin);
						break;
					
					case Barrier.Types.Water:
						Speed = new Vector2(0, 8);
						
						Barrier.UpdateFrame(0, 1, [1, 2]);
						Barrier.UpdateDuration([6, 18]);
						Barrier.AnimationTimer = 25f;
						
						//TODO: audio
						//audio_play_sfx(sfx_barrier_water2);
						break;
					
					case Barrier.Types.Flame:
						if (!SharedData.CDCamera)
						{
							Camera.MainCamera.Delay.X = 16;
						}
						
						IsAirLock = true;
						Speed = new Vector2(8f * (float)Facing, 0f);
							

						// TODO: SetAnimation
						//Barrier.SetAnimation(, [2]);
						// TODO: depth
						//obj_set_priority(1);
						
						Barrier.AnimationTimer = 24f;
						
						//TODO: audio
						//audio_play_sfx(sfx_barrier_flame2);
						break;
					
					case Barrier.Types.Thunder:
						Barrier.State = Barrier.States.Disabled;
						Speed.Y = -5.5f;
						
						for (var i = 0; i < 4; i++)
						{
							//TODO: obj_barrier_sparkle
							//instance_create(x, y, obj_barrier_sparkle, { Sparkle_ID: i });
						}
						//TODO: audio
						//audio_play_sfx(sfx_barrier_thunder2);
						break;
				}
				break;
			
			case Types.Tails:
				if (Action > 0 || !InputPress.Abc) break;
				
				IsAirLock = false;
				IsSpinning = false;
				IsJumping = false;
				Gravity	= GravityType.TailsDown;
				Action = Actions.Flight;
				ActionValue = 480;
				
				Radius = RadiusNormal;
				
				if (!IsUnderwater)
				{
					//TODO: audio
					//audio_play_sfx(sfx_flight, true);
				}
					
				InputDown.Abc = false;
				InputPress.Abc = false;
				break;
			
			case Types.Knuckles:
				if (Action > 0 || !InputPress.Abc) break;
				
				IsAirLock = false;
				IsSpinning = false;
				IsJumping = false;	
				Animation = Animations.GlideAir;	
				Action = Actions.Glide;
				ActionState = (int)GlideStates.Air;
				ActionValue = Facing == Constants.Direction.Negative ? 0f : 180f;
				Radius = new Vector2I(10, 10);
				GroundSpeed = 4f;
				Speed = new Vector2(0f, Speed.Y + 2f);
				
				if (Speed.Y < 0)
				{
					Speed.Y = 0;
				}
				break;
			
			case Types.Amy:
				if (Action > 0 || !InputPress.Abc) break;
				
				Speed.Y = PhysicParams.MinimalJumpVelocity;
				IsAirLock = false;
				Animation = Animations.HammerSpin;
				Action = Actions.HammerSpin;
				ActionValue = 0;
				// TODO: audio
				//audio_play_sfx(sfx_hammer_spin);
				break;
		}

		return false;
	}

	private bool StartJump()
	{
		if (Action == Actions.SpinDash || Action == Actions.PeelOut || IsForcedRoll || !IsGrounded) return false;
		
		if (!InputPress.Abc) return false;
	
		const int maxCeilingDist = 6;
		var position = (Vector2I)Position;
		int ceilDist = GroundMode switch
		{
			Constants.GroundMode.Floor => CollisionUtilities.FindTileTwoPositions(true, position - Radius, position + new Vector2I(Radius.X, -Radius.Y), Constants.Direction.Negative, TileLayer, GroundMode).Item1,
			Constants.GroundMode.RightWall => CollisionUtilities.FindTileTwoPositions(true, x - Radius.Y, y - Radius.X, x - Radius.Y, y + Radius.X, Constants.Direction.Negative, TileLayer, GroundMode).Item1,
			Constants.GroundMode.LeftWall => CollisionUtilities.FindTileTwoPositions(true, x + Radius.Y, y - Radius.X, x + Radius.Y, y + Radius.X, Constants.Direction.Positive, TileLayer, GroundMode).Item1,
			_ => maxCeilingDist
		};
	
		if ceilDist < maxCeilingDist
		{
			return;
		}
	
		// ???
		if !SharedData.fix_jump_size
		{
			Radius.X = radius_x_normal;
			Radius.Y = radius_y_normal;
		}
	
		if !IsSpinning
		{	
			y += Radius.Y - radius_y_spin;
			Radius.X = radius_x_spin;
			Radius.Y = radius_y_spin;
		}
		else if !SharedData.no_roll_lock && SharedData.PlayerPhysics != PHYSICS_CD
		{
			IsAirLock = true;
		}
	
		Speed.X += jump_vel * dsin(angle);
		Speed.Y += jump_vel * dcos(angle);
		IsSpinning = true;	
		IsJumping = true;
		is_pushing = false;
		IsGrounded = false;
		is_on_object = false;
		stick_to_convex = false;
		GroundMode = 0;
	
		if Type == Types.Amy
		{
			obj_set_hitbox_ext(25, 25);
			Animation = Animations.HammerSpin;
		}
		else
		{
			Animation = Animations.Spin;
		}
	 
		audio_play_sfx(sfx_jump);
	
		// return player control routine
		return true;
	}

	private void ProcessDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash)
		{
			return;
		}
	
		var MAX_DROPDASH_CHARGE = 20;
	
		if !IsGrounded
		{		
			var InputDown = player_get_input(1);
			if InputDown.Abc
			{
				IsAirLock = false;		
				if ActionValue < MAX_DROPDASH_CHARGE
				{
					ActionValue++;
				}
				else 
				{
					if Animation != ANI_DROPDASH
					{
						Animation = ANI_DROPDASH;
						audio_play_sfx(sfx_charge);
					}
				}
			}
			else if ActionValue > 0
			{
				if ActionValue == MAX_DROPDASH_CHARGE
				{		
					Animation = Animations.Spin;
					Action = ACTION_DROPDASH_C;
				}
			
				ActionValue = 0;
			}
		}
	
		// Called from player_land() function
		else if ActionValue == MAX_DROPDASH_CHARGE
		{
			y += Radius.Y - radius_y_spin;
			Radius.X = radius_x_spin;
			Radius.Y = radius_y_spin;
		
			var _force = 8;
			var _max_speed = 12;
		
			if IsSuper
			{
				_force = 12;
				_max_speed = 13;
				Camera.MainCamera.shake_timer = 6;
			}
		
			if Facing == FLIP_LEFT
			{
				if Speed.X <= 0 
				{
					GroundSpeed = (GroundSpeed >> 2) - _force;	// floor(GroundSpeed / 4)
					if GroundSpeed < -_max_speed
					{
						GroundSpeed = -_max_speed;
					}
				}
				else if angle != 360 
				{
					GroundSpeed = (GroundSpeed >> 1) - _force;	// floor(GroundSpeed / 2)
				}
				else
				{
					GroundSpeed = -_force;
				}
			}
			else 
			{
				if Speed.X >= 0
				{
					GroundSpeed = (GroundSpeed >> 2) + _force;
					if GroundSpeed > _max_speed
					{
						GroundSpeed = _max_speed;
					}
				}
				else if angle != 360
				{
					GroundSpeed = (GroundSpeed >> 1) + _force;
				}
				else 
				{
					GroundSpeed = _force;
				}
			}
		
			Animation = Animations.Spin;
			IsSpinning = true;
		
			if !SharedData.CDCamera
			{
				Camera.MainCamera.Delay.X = 8;
			}
		
			instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
			audio_stop_sfx(sfx_charge);
			audio_play_sfx(sfx_release);	
		}
	}
	
	private void ProcessFlight()
	{
		if Action != Actions.Flight
		{
			return;
		}
	
		var InputPress = player_get_input(0);
		var InputDown = player_get_input(1);
	
		if ActionValue > 0
		{
			if --ActionValue == 0
			{
				if !IsUnderwater
				{
					Animation = ANI_FLY_TIRED;
					audio_play_sfx(sfx_flight2, true);
				}
				else
				{
					Animation = ANI_SWIM_TIRED;
				}
			
				Gravity = GravityType.TailsDown;
				audio_stop_sfx(sfx_flight);
			}
			else
			{	
				if !IsUnderwater
				{
					Animation = carry_target == noone ? ANI_FLY : ANI_FLY_LIFT;
				}
				else
				{
					Animation = carry_target == noone ? ANI_SWIM : ANI_SWIM_LIFT;
				}
			
				if !(IsUnderwater && carry_target != noone)
				{
					if InputPress.Abc
					{
						Gravity = GRV_TAILS_UP;
					}
					else if Speed.Y < -1
					{
						Gravity = GravityType.TailsDown;
					}
				
					Speed.Y = Math.Max(Speed.Y, -4);
				}
			}
		}
	
		if SharedData.flight_cancel && InputDown.down && InputPress.Abc
		{	
			Camera.MainCamera.view_y += Radius.Y - radius_y_spin;
			Radius.X = radius_x_spin;
			Radius.Y = radius_y_spin;
			Animation = Animations.Spin;
			IsSpinning	= true;
			Action = Actions.None;
		
			audio_stop_sfx(sfx_flight);
			audio_stop_sfx(sfx_flight2);
			player_reset_gravity(id);
		}
	}
	
	private void ProcessClimb()
	{
		if Action != ACTION_CLIMB
		{
			return;
		}
		
		var InputPress = player_get_input(0);
		var InputDown = player_get_input(1);
		
		switch ActionState
		{
			case CLIMB_STATE_NORMAL:
			
				if x != xprevious
				{
					sub_PlayerClimbRelease();
					break;
				}
		
				var STEPS_PER_FRAME = 4;
				var _max_value = image_number * STEPS_PER_FRAME;
				
				if InputDown.Up
				{
					if ++ActionValue > _max_value
					{
						ActionValue = 0;
					}
					
					Speed.Y = -acc_climb;
				}
				else if InputDown.down
				{
					if --ActionValue < 0
					{
						ActionValue = _max_value;
					}
					
					Speed.Y = acc_climb;
				}
				else
				{
					Speed.Y = 0;
				}
				
				var _radius_x = Radius.X;
				if Facing == FLIP_LEFT
				{
					_radius_x++;
				}
		
				if Speed.Y < 0
				{
					// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
					var _wall_dist = tile_find_h(x + _radius_x * Facing, y - Radius.Y - 1, Facing, TileLayer)[0];
					if _wall_dist >= 4
					{
						ActionState = CLIMB_STATE_LEDGE;
						ActionValue = 0;
						Speed.Y = 0;
						
						break;
					}
		
					// If Knuckles has encountered a small dip in the wall, cancel climb movement
					if _wall_dist != 0
					{
						Speed.Y = 0;
					}
		
					// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
					var _ceil_dist = tile_find_v(x + _radius_x * Facing, y - radius_y_normal + 1, false, TileLayer)[0];
					if _ceil_dist < 0
					{
						y -= _ceil_dist;
						Speed.Y = 0;
					}
				}
				else
				{
					// If Knuckles is no longer against the wall, make him let go
					var _wall_dist = tile_find_h(x + _radius_x * Facing, y + Radius.Y + 1, Facing, TileLayer)[0];
					if _wall_dist != 0
					{
						sub_PlayerClimbRelease();
						break;
					}
					
					// If Knuckles has reached the floor, make him land
					var _floor_data = tile_find_v(x + _radius_x * Facing, y + radius_y_normal, true, TileLayer);
					var _floor_dist = _floor_data[0];
					var _floor_angle = _floor_data[1];
					
					if _floor_dist < 0
					{
						y += _floor_dist + radius_y_normal - Radius.Y;
						angle = _floor_angle;
						
						player_land(id);

						Animation = ANI_IDLE;
						Speed.Y = 0;
						
						break;
					}
				}
				
				if InputPress.Abc
				{
					Animation = Animations.Spin;
					IsSpinning = true;
					IsJumping = true;		
					Action = Actions.None;
					Facing *= -1;
					Speed.X = 3.5 * Facing;
					Speed.Y = PhysicParams.MinimalJumpVelocity;
					
					audio_play_sfx(sfx_jump);
					player_reset_gravity(id);	
				}
				else if Speed.Y != 0
				{
					ani_upd_frame(floor(ActionValue / STEPS_PER_FRAME));
				}
				
			break;
			
			case CLIMB_STATE_LEDGE:
			
				switch ActionValue
				{
					// Frame 0
					case 0:
					
						Animation = ANI_CLIMB_LEDGE;
						x += 3 * Facing;
						y -= 2;
						
					break;
					
					// Frame 1
					case 6:
					
						x += 8 * Facing;
						y -= 10;
						
					break;
					
					// Frame 2
					case 12:
					
						x -= 8 * Facing;
						y -= 12;
						
					break;
					
					// End
					case 18:
					
						player_land(id);
						
						Animation = ANI_IDLE;
						x += 8 * Facing;
						y += 4;
						
					break;
				}
				
				ActionValue++;
				
			break;
		}
	}

	private void ReleaseClimb()
	{
		Animation = ANI_GLIDE_FALL;
		Action = Actions.Glide;
		ActionState = GLIDE_STATE_FALL;
		ActionValue = 1;
		Radius.X = radius_x_normal;
		Radius.Y = radius_y_normal;
		
		player_reset_gravity(id);
	}
	
	private void ProcessGlide()
	{
		if Action != Actions.Glide || ActionState == GLIDE_STATE_FALL
		{
			return;
		}
		
		var ANGLE_INC = 2.8125;
		var GLIDE_GRV = 0.125;
		var SLIDE_FRC = 0.09375;
		var InputDown = player_get_input(1);
		
		switch ActionState
		{
			case GLIDE_STATE_AIR:
				
				// ground vel - glide speed
				// ActionValue - glide angle
				
				// Update glide speed
				if GroundSpeed >= 4
				{
					if ActionValue % 180 == 0
					{
						GroundSpeed = min(GroundSpeed + acc_glide, 24);
					}
				}
				else
				{
					GroundSpeed += 0.03125;
				}
				
				// Turn around
				if ActionValue != 0 && InputDown.Left
				{
					if ActionValue > 0
					{
						ActionValue = -ActionValue;
					}
					
					ActionValue += ANGLE_INC;
				}
				else if ActionValue != 180 && InputDown.Right
				{
					if ActionValue < 0
					{
						ActionValue = -ActionValue;
					}
					
					ActionValue += ANGLE_INC;
				}
				else if ActionValue % 180 != 0
				{
					ActionValue += ANGLE_INC;
				}
				
				// Update horizontal and vertical speed
				Speed.X = GroundSpeed * -dcos(ActionValue);
				if Speed.Y < 0.5
				{
					Gravity = GLIDE_GRV;
				}
				else
				{
					Gravity = -GLIDE_GRV;
				}
				
				// Update Animation frame
				var _angle = abs(ActionValue) % 180;
				if _angle < 30 || _angle > 150
				{
					ani_upd_frame(0);
				}
				else if _angle < 60 || _angle > 120
				{
					ani_upd_frame(1);
				}
				else
				{	
					Facing = _angle < 90 ? FLIP_LEFT : FLIP_RIGHT;
					ani_upd_frame(2);
				}
				
				if !InputDown.Abc
				{
					Animation = ANI_GLIDE_FALL;
					ActionState = GLIDE_STATE_FALL;
					ActionValue = 0;
					Radius.X = radius_x_normal;
					Radius.Y = radius_y_normal;
					Speed.X *= 0.25;

					player_reset_gravity(id);
				}
				
			break;
			
			case GLIDE_STATE_GROUND:
				
				if !InputDown.Abc
				{
					Speed.X = 0;
				}
				else if Speed.X > 0
				{
					Speed.X = Math.Max(0, Speed.X - SLIDE_FRC);
				}
				else if Speed.X < 0
				{
					Speed.X = min(0, Speed.X + SLIDE_FRC);
				}

				if Speed.X == 0
				{
					player_land(id);
					ani_upd_frame(1);

					Animation = ANI_GLIDE_GROUND;
					ground_lock_timer = 16;
					GroundSpeed = 0;

					break;
				}

				if ActionValue % 4 == 0
				{
					instance_create(x, y + Radius.Y, obj_dust_skid);
				}
				
				if ActionValue > 0 && ActionValue % 8 == 0
				{
					audio_play_sfx(sfx_slide);
				}
					
				ActionValue++;
				
			break;
		}
	}
	
	private void ProcessHammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
	
		var MAX_HAMMERSPIN_CHARGE = 20;
		var InputDown = player_get_input(1);
	
		if !InputDown.Abc
		{
			if ActionValue >= MAX_HAMMERSPIN_CHARGE
			{
				Action = ACTION_HAMMERSPIN_C;
			}
		
			ActionValue = 0;
			Animation = Animations.Spin;
		
			return;
		}

		if ActionValue < MAX_HAMMERSPIN_CHARGE
		{
			if ActionValue == 0
			{
				obj_set_hitbox_ext(25, 25);
				Animation = Animations.HammerSpin;
			}
			
			ActionValue++;
			if ActionValue != MAX_HAMMERSPIN_CHARGE
			{
				return;
			}
		
			audio_play_sfx(sfx_charge);
		}
		
		// Called from player_land() function
		if !IsGrounded
		{
			return;
		}
	
		Animation = ANI_HAMMERRUSH;
		Action = Actions.HammerRush;
		ActionValue = 59; // (60)
			
		audio_stop_sfx(sfx_charge);
		audio_play_sfx(sfx_release);
	}
	
	private void ProcessHammerRush()
	{
		if Action != Actions.HammerRush
		{
			return;
		}
	
		switch ani_get_frame() % 4
		{
			case 0:
			obj_set_hitbox_ext(16, 16, 6 * Facing, 0);
			break;
			
			case 1:
			obj_set_hitbox_ext(16, 16, -7 * Facing, 0);
			break;
			
			case 2:
			obj_set_hitbox_ext(14, 20, -4 * Facing, -4);
			break;
			
			case 3:
			obj_set_hitbox_ext(17, 21, 7 * Facing, -5);
			break;
		}
	
		var InputDown = player_get_input(1)
		if IsGrounded
		{
			if !InputDown.Abc || ActionValue == 0 || GroundSpeed == 0 || dcos(angle) <= 0
			{
				sub_PlayerHammerRushCancel();
				return;
			}
		
			ActionValue--;
		
			if InputDown.Left && GroundSpeed > 0 || InputDown.Right && GroundSpeed < 0
			{
				Facing *= -1;
				GroundSpeed *= -1;
			}
		
			Speed.X = GroundSpeed * dcos(angle);
			Speed.Y = GroundSpeed * -dsin(angle);
		}
		else if Speed.X == 0
		{
			sub_PlayerHammerRushCancel();
		}
		else
		{
			Speed.X = 6 * sign(GroundSpeed);
		}
	}

	private void CancelHammerRush()
	{
		Animation = Animations.Move;
		Action = Actions.None;
	}

	private void ProcessSlopeResist()
	{
		if !IsGrounded || IsSpinning || angle > 135 && angle <= 225
		{
			return;
		}
	
		if Action == Actions.HammerRush || Action == Actions.PeelOut
		{
			return;
		}
	
		var _slope_grv = 0.125 * dsin(angle);
		if GroundSpeed != 0 || SharedData.PlayerPhysics >= PhysicsTypes.S3 && abs(_slope_grv) > 0.05078125
		{
			GroundSpeed -= _slope_grv;
		}
	}

	private void ProcessSlopeResistRoll()
	{
		if !IsGrounded || !IsSpinning || angle > 135 && angle <= 225
		{
			return;
		}
	
		var _slope_grv = sign(GroundSpeed) != sign(dsin(angle)) ? 0.3125 : 0.078125;
		GroundSpeed -= _slope_grv * dsin(angle);
	}

	private void ProcessMovementGround()
	{
		// Control routine checks
		if (!IsGrounded || IsSpinning) return;
		
		// Action checks
		if (Action is Actions.SpinDash or Actions.PeelOut or Actions.HammerRush) return;
		
		var InputDown = player_get_input(1);
		var _do_skid = false;
		
		// If Knuckles is standing up from a slide and DOWN button is pressed, cancel
		// control lock. This allows him to Spin Dash
		if Animation == ANI_GLIDE_GROUND && InputDown.down
		{
			ground_lock_timer = 0;
		}
		
		if ground_lock_timer == 0
		{
			// Move left
			if InputDown.Left
			{	
				if GroundSpeed > 0 
				{
					GroundSpeed -= dec;
					if GroundSpeed <= 0
					{
						GroundSpeed = -0.5;
					}
					
					_do_skid = true;
				}
				else
				{
					if !SharedData.no_speed_cap || GroundSpeed > -PhysicParams.AccelerationTop
					{
						GroundSpeed = Math.Max(GroundSpeed - acc, -PhysicParams.AccelerationTop);
					}
					
					if Facing != FLIP_LEFT
					{
						Animation = Animations.Move;
						Facing = FLIP_LEFT;
						is_pushing = false;
						
						ani_upd_frame(0);
					}
				}
			}
			
			// Move right
			if InputDown.Right
			{
				if GroundSpeed < 0 
				{
					GroundSpeed += dec;
					if GroundSpeed >= 0
					{
						GroundSpeed = 0.5;
					}
					
					_do_skid = true;
				} 
				else
				{
					if !SharedData.no_speed_cap || GroundSpeed < PhysicParams.AccelerationTop
					{
						GroundSpeed = min(GroundSpeed + acc, PhysicParams.AccelerationTop);
					}
					
					if Facing != FLIP_RIGHT
					{
						Animation = Animations.Move;
						Facing = FLIP_RIGHT;
						is_pushing = false;
						
						ani_upd_frame(0);
					}
				}
			}
			
			// Set idle / move / skid Animation
			if math_get_angle_quad(angle) == 0
			{
				if _do_skid && abs(GroundSpeed) >= 4 && Animation != ANI_SKID
				{
					AnimationTimer = Type == Types.Sonic ? 24 : 16;
					Animation = ANI_SKID;
					
					audio_play_sfx(sfx_skid);
				}
				
				if GroundSpeed == 0
				{
					if InputDown.Up
					{
						Animation = Animations.LookUp;
					}
					else if InputDown.down
					{
						Animation = ANI_DUCK;
					}
					else
					{
						Animation = ANI_IDLE;
					}
					
					is_pushing = false;
				}
				
				// TODO: This
				else if Animation != ANI_SKID && Animation != ANI_PUSH
				{
					Animation = Animations.Move;
				}
			}
			
			else if Animation != ANI_SKID && Animation != ANI_PUSH
			{
				Animation = Animations.Move;
			}
			
			// Set / clear push Animation
			if is_pushing
			{
				if Animation == Animations.Move && ani_get_timer() == ani_get_duration()
				{
					Animation = ANI_PUSH;
				}
			}
			else if Animation == ANI_PUSH
			{
				Animation = Animations.Move
			}
		}
		
		// Apply friction
		if !InputDown.Left && !InputDown.Right
		{
			if GroundSpeed > 0
			{
				GroundSpeed = Math.Max(GroundSpeed - frc, 0);
			}
			else if GroundSpeed < 0
			{
				GroundSpeed = min(GroundSpeed + frc, 0);
			}
		}
		
		// Convert ground velocity into directional velocity
		Speed.X = GroundSpeed * dcos(angle);
		Speed.Y = GroundSpeed * -dsin(angle);
	}

	private void ProcessMovementGroundRoll()
	{
		// Control routine checks
		if !IsGrounded || !IsSpinning
		{
			return;
		}
	
		var InputDown = player_get_input(1);
	
		if ground_lock_timer == 0
		{
			// Move left
			if InputDown.Left
			{
				if GroundSpeed > 0
				{
					GroundSpeed -= dec_roll;
					if GroundSpeed < 0
					{
						GroundSpeed = -0.5;
					}
				}
				else
				{
					Facing = FLIP_LEFT;
					is_pushing = false;	
				}
			}
		
			// Move right
			if InputDown.Right
			{
				if GroundSpeed < 0
				{
					GroundSpeed += dec_roll;
					if GroundSpeed >= 0
					{
						GroundSpeed = 0.5;
					}
				}
				else
				{
					Facing = FLIP_RIGHT;
					is_pushing = false;	
				}
			}
		}
	
		// Apply friction
		if GroundSpeed > 0
		{
			GroundSpeed = Math.Max(GroundSpeed - frc_roll, 0);
		}
		else if GroundSpeed < 0
		{
			GroundSpeed = min(GroundSpeed + frc_roll, 0);
		}
	
		// Stop spinning
		if !IsForcedRoll
		{
			if GroundSpeed == 0 || SharedData.PlayerPhysics == PHYSICS_SK && abs(GroundSpeed) < 0.5
			{
				y += Radius.Y - radius_y_normal;
			
				Radius.X = radius_x_normal;
				Radius.Y = radius_y_normal;
			
				IsSpinning = false;
				Animation = ANI_IDLE;
			}
		}
	
		// If forced to spin, keep moving player
		else
		{
			if SharedData.PlayerPhysics == PHYSICS_CD
			{
				if GroundSpeed >= 0 && GroundSpeed < 2
				{
					GroundSpeed = 2;
				}
			}
			else if GroundSpeed == 0
			{
				GroundSpeed = SharedData.PlayerPhysics == PHYSICS_S1 ? 2 : 4 * Facing;
			}
		}
	
		Speed.X = GroundSpeed * dcos(angle);
		Speed.Y = GroundSpeed * -dsin(angle);
	
		Speed.X = Math.Clamp(Speed.X, -16, 16);
	}

	private void ProcessMovementAir()
	{
		// Control routine checks
		if IsGrounded || is_dead
		{
			return;
		}
	
		// Action checks
		if Action == ACTION_CARRIED || Action == ACTION_CLIMB 
			|| Action == Actions.Glide && ActionState != GLIDE_STATE_FALL
		{
			return;
		}
	
		// Update angle (rotate player)
		if angle != 360
		{
			if angle >= 180
			{
				angle += 2.8125;
			}
			else
			{
				angle -= 2.8125;
			}
		
			if angle <= 0 || angle > 360
			{
				angle = 360;
			}
		}
	
		// Limit upward speed
		if !IsJumping && Action != Actions.SpinDash && !IsForcedRoll
		{
			if Speed.Y < -15.75
			{
				Speed.Y = -15.75;
			}
		}
	
		// Limit downward speed
		if SharedData.PlayerPhysics == PHYSICS_CD && Speed.Y > 16
		{
			Speed.Y = 16;
		}
	
		if !IsAirLock
		{
			var InputDown = player_get_input(1);
		
			// Move left
			if InputDown.Left
			{
				if Speed.X > 0 
				{
					Speed.X -= acc_air;
				}
				else if !SharedData.no_speed_cap || Speed.X > -PhysicParams.AccelerationTop
				{
					Speed.X = Math.Max(Speed.X - acc_air, -PhysicParams.AccelerationTop);
				}
			
				Facing = FLIP_LEFT;
			}
		
			// Move right
			if InputDown.Right
			{
				if Speed.X < 0 
				{
					Speed.X += acc_air;
				} 
				else if !SharedData.no_speed_cap || Speed.X < PhysicParams.AccelerationTop
				{
					Speed.X = min(Speed.X + acc_air, PhysicParams.AccelerationTop);
				}
			
				Facing = FLIP_RIGHT;
			}	
		}
	
		// Apply air drag
		if !is_hurt && Speed.Y < 0 && Speed.Y > -4
		{
			Speed.X -= floor(Speed.X / 0.125) / 256;
		}
	}

	private void ProcessBalance()
	{
		if !IsGrounded || IsSpinning
		{
			return;
		}
	
		if GroundSpeed != 0 || Action == Actions.SpinDash || Action == Actions.PeelOut
		{
			return;
		}
	
		if SharedData.PlayerPhysics == PHYSICS_SK
		{
			var InputDown = player_get_input(1);	
			if InputDown.down || InputDown.Up && SharedData.PeelOut
			{
				return;
			}	
		}
	
		if !is_on_object
		{
			if math_get_angle_quad(angle) > 0
			{
				return;
			}
		
			var _floor_dist = tile_find_v(x, y + Radius.Y, true, TileLayer)[0];	
			if _floor_dist < 12
			{
				return;
			}
		
			var _angle_left = tile_find_v(x - Radius.X, y + Radius.Y, true, TileLayer)[1];
			var _angle_right = tile_find_v(x + Radius.X, y + Radius.Y, true, TileLayer)[1];
		
			if _angle_left != noone && _angle_right != noone
			{
				return;
			}
		
			if _angle_left == noone
			{	
				var _left_dist = tile_find_v(x + 6, y + Radius.Y, true, TileLayer)[0];
				sub_PlayerBalanceLeft(_left_dist >= 12);
			}
			else
			{
				var _right_dist = tile_find_v(x - 6, y + Radius.Y, true, TileLayer)[0];
				sub_PlayerBalanceRight(_right_dist >= 12);
			}
		}
		else if instance_exists(is_on_object)
		{
			if is_on_object.data_solid.no_balance
			{
				return;
			}
		
			var _player_x = floor(is_on_object.data_solid.Radius.X - is_on_object.x + x);
			var _left_edge = 2;
			var _right_edge = is_on_object.data_solid.Radius.X * 2 - _left_edge;
		
			if _player_x < _left_edge
			{
				_left_edge -= 4;
				sub_PlayerBalanceLeft(_player_x < _left_edge);
			}
			else if _player_x > _right_edge
			{
				_right_edge += 4;
				sub_PlayerBalanceRight(_player_x > _left_edge);
			}
		}
	}

	private void BalanceLeft(bool isPanic)
	{
		if Type != Types.Sonic || IsSuper
		{
			Animation = ANI_BALANCE;
			Facing = FLIP_LEFT;
			
			return;
		}
		
		if !isPanic
		{
			Animation = (Facing == FLIP_LEFT) ? ANI_BALANCE : ANI_BALANCE_FLIP;
		}
		else if Facing == FLIP_RIGHT
		{
			Animation = ANI_BALANCE_TURN;
			Facing = FLIP_LEFT;
		}
		else if Animation != ANI_BALANCE_TURN
		{
			Animation = ANI_BALANCE_PANIC;
		}
	}

	private void BalanceRight(bool isPanic)
	{
		if Type != Types.Sonic || IsSuper
		{
			Animation = ANI_BALANCE;
			Facing = FLIP_RIGHT;
			
			return;
		}
		
		if !isPanic
		{
			Animation = (Facing == FLIP_RIGHT) ? ANI_BALANCE : ANI_BALANCE_FLIP;
		}
		else if Facing == FLIP_LEFT
		{
			Animation = ANI_BALANCE_TURN;
			Facing = FLIP_RIGHT;
		}
		else if Animation != ANI_BALANCE_TURN
		{
			Animation = ANI_BALANCE_PANIC;
		}
	}

	private void ProcessCollisionGroundWalls()
	{
		// Control routine checks
		if !IsGrounded
		{
			return;
		}
		
		if SharedData.PlayerPhysics < PHYSICS_SK
		{
			if angle > 90 && angle <= 270
			{
				return;
			}
		}
		else if angle >= 90 && angle <= 270 && angle % 90 != 0
		{
			return;
		}
		
		var _cast_dir = 0;
		if angle >= 45 && angle <= 128
		{
			_cast_dir = 1;
		}
		else if angle > 128 && angle < 225
		{
			_cast_dir = 2;
		}
		else if angle >= 225 && angle < 315
		{
			_cast_dir = 3;
		}
		
		var _wall_radius = radius_x_normal + 1;
		var _y_offset = 8 * (angle == 360);
		
		if GroundSpeed < 0
		{
			var _wall_dist = 0;
			switch _cast_dir
			{
				case 0:
					_wall_dist = tile_find_h(x + Speed.X - _wall_radius, y + Speed.Y + _y_offset, false, TileLayer, GroundMode)[0];
				break;
				
				case 1:
					_wall_dist = tile_find_v(x + Speed.X, y + Speed.Y + _wall_radius, true, TileLayer, GroundMode)[0];
				break;
				
				case 2:
					_wall_dist = tile_find_h(x + Speed.X + _wall_radius, y + Speed.Y, true, TileLayer, GroundMode)[0];
				break;
				
				case 3:
					_wall_dist = tile_find_v(x + Speed.X, y + Speed.Y - _wall_radius, false, TileLayer, GroundMode)[0];
				break;
			}
			
			if _wall_dist >= 0
			{
				return;
			}
			
			switch math_get_angle_quad(angle)
			{
				case 0:
					
					Speed.X -= _wall_dist;
					GroundSpeed = 0;
						
					if Facing == FLIP_LEFT && !IsSpinning
					{
						is_pushing = true;
					}
						
				break;
					
				case 1:
					Speed.Y += _wall_dist;
				break;
					
				case 2:
					
					Speed.X += _wall_dist;
					GroundSpeed = 0;
						
					if Facing == FLIP_LEFT && !IsSpinning
					{
						is_pushing = true;
					}
						
				break;
					
				case 3:
					Speed.Y -= _wall_dist;
				break;
			}
		}
		else if GroundSpeed > 0
		{
			var _wall_dist = 0;
			switch _cast_dir
			{
				case 0:
					_wall_dist = tile_find_h(x + Speed.X + _wall_radius, y + Speed.Y + _y_offset, true, TileLayer, GroundMode)[0];
				break;
				
				case 1:
					_wall_dist = tile_find_v(x + Speed.X, y + Speed.Y - _wall_radius, false, TileLayer, GroundMode)[0];
				break;
				
				case 2:
					_wall_dist = tile_find_h(x + Speed.X - _wall_radius, y + Speed.Y, false, TileLayer, GroundMode)[0];
				break;
				
				case 3:
					_wall_dist = tile_find_v(x + Speed.X, y + Speed.Y + _wall_radius, true, TileLayer, GroundMode)[0];
				break;
			}
			
			if _wall_dist >= 0
			{
				return;
			}
			
			switch math_get_angle_quad(angle)
			{
				case 0:
					
					Speed.X += _wall_dist;
					GroundSpeed = 0;
						
					if Facing == FLIP_RIGHT && !IsSpinning
					{
						is_pushing = true;
					}
					
				break;
					
				case 1:
					Speed.Y -= _wall_dist;
				break;
					
				case 2:
					
					Speed.X -= _wall_dist;
					GroundSpeed = 0;
						
					if Facing == FLIP_RIGHT && !IsSpinning
					{
						is_pushing = true;
					}
						
				break;
						
				case 3:
					Speed.Y += _wall_dist;
				break;
			}
		}
	}

	private void ProcessRollStart()
	{
		if (!IsGrounded || IsSpinning || Action == Actions.SpinDash || Action == Actions.HammerRush) return;
		
		if (!IsForcedRoll && (InputDown.Left || InputDown.Right))
		{
			return;
		}
	
		var _allow_spin = false;
		if InputDown.down
		{
			if SharedData.PlayerPhysics == PHYSICS_SK
			{
				if abs(GroundSpeed) >= 1
				{
					_allow_spin = true;
				}
				else
				{
					Animation = ANI_DUCK;
				}
			}
			else if abs(GroundSpeed) >= 0.5
			{
				_allow_spin = true;
			}
		}
	
		if _allow_spin || IsForcedRoll
		{
			y += Radius.Y - radius_y_spin;
			Radius.Y = radius_y_spin;
			Radius.X = radius_x_spin;
			IsSpinning = true;
			Animation = Animations.Spin;

			audio_play_sfx(sfx_roll);
		}
	}

	private void ProcessLevelBound()
	{
		if is_dead
		{
			return;
		}
	
		var _camera = Camera.MainCamera;
	
		// Note that position here is checked including subpixel
		if x + Speed.X < _camera.view_x_min + 16
		{
			GroundSpeed = 0;
			Speed.X = 0;
			x = _camera.view_x_min + 16;
		}
	
		var _right_bound = _camera.view_x_max - 24;
		if instance_exists(obj_signpost)
		{
			// TODO: There should be a better way?
			_right_bound += 64;
		}
	
		if x + Speed.X > _right_bound
		{
			GroundSpeed = 0;
			Speed.X = 0;
			x = _right_bound;
		}
	
		if Action == Actions.Flight || Action == ACTION_CLIMB
		{
			if y + Speed.Y < _camera.view_y_min + 16
			{ 	
				if Action == Actions.Flight
				{
					Gravity	= GravityType.TailsDown;
				}
			
				Speed.Y = 0;
				y = _camera.view_y_min + 16;
			}
		}	
		else if Action == Actions.Glide && y < _camera.view_y_min + 10
		{
			Speed.X = 0;
		}
	
		if air_timer > 0 && y > Math.Max(_camera.view_y_max, _camera.bound_bottom)
		{
			player_kill(id);
		}
	}

	private void ProcessPosition()
	{
		if Action == ACTION_CARRIED
		{
			return;
		}
	
		if stick_to_convex
		{
			Speed.X = Math.Clamp(Speed.X, -16, 16);
			Speed.Y = Math.Clamp(Speed.Y, -16, 16);
		}
	
		x += Speed.X;
		y += Speed.Y;
	
		if !IsGrounded && Action != ACTION_CARRIED
		{
			Speed.Y += Gravity;
		}
	}

	private void ProcessCollisionGroundFloor()
	{
		// Control routine checks
		if !IsGrounded || is_on_object
		{
			return;
		}
		
		if angle <= 45 || angle >= 315
		{
			GroundMode = 0;	// Floor
		}
		else if angle > 45 && angle < 135
		{
			GroundMode = 1;	// Right wall
		}
		else if angle >= 135 && angle <= 225
		{
			GroundMode = 2;	// Ceiling
		}
		else
		{
			GroundMode = 3;	// Left wall
		}

		var _MIN_TOLERANCE = 4;
		var _MAX_TOLERANCE = 14;
		
		switch GroundMode
		{
			// Floor
			case 0:
			
				var _floor_data = tile_find_2v(x - Radius.X, y + Radius.Y, x + Radius.X, y + Radius.Y, true, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if !stick_to_convex
				{
					var _tolerance = SharedData.PlayerPhysics < PHYSICS_S2 ? _MAX_TOLERANCE : min(_MIN_TOLERANCE + abs(floor(Speed.X)), _MAX_TOLERANCE);
					if _floor_dist > _tolerance
					{
						is_pushing = false;
						IsGrounded = false;
						
						ani_upd_frame(0);
						break;
					}		
				}

				if _floor_dist < -_MAX_TOLERANCE
				{
					break;
				}
				
				if SharedData.PlayerPhysics >= PHYSICS_S2
				{
					_floor_angle = sub_PlayerSnapFloorAngle(_floor_angle);
				}
				
				y += _floor_dist;
				angle = _floor_angle;
			
			break;

			// Right wall
			case 1:
			
				var _floor_data = tile_find_2h(x + Radius.Y, y + Radius.X, x + Radius.Y, y - Radius.X, true, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if !stick_to_convex
				{
					var _tolerance = SharedData.PlayerPhysics < PHYSICS_S2 ? _MAX_TOLERANCE : min(_MIN_TOLERANCE + abs(floor(Speed.Y)), _MAX_TOLERANCE);
					if _floor_dist > _tolerance
					{
						is_pushing = false;
						IsGrounded = false;
						
						ani_upd_frame(0);
						break;
					}	
				}
				
				if _floor_dist < -_MAX_TOLERANCE
				{
					break;
				}

				if SharedData.PlayerPhysics >= PHYSICS_S2
				{
					_floor_angle = sub_PlayerSnapFloorAngle(_floor_angle);
				}
				
				x += _floor_dist;
				angle = _floor_angle;

			break;

			// Ceiling
			case 2:
			
				var _floor_data = tile_find_2v(x + Radius.X, y - Radius.Y, x - Radius.X, y - Radius.Y, false, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if !stick_to_convex
				{
					var _tolerance = SharedData.PlayerPhysics < PHYSICS_S2 ? _MAX_TOLERANCE : min(_MIN_TOLERANCE + abs(floor(Speed.X)), _MAX_TOLERANCE);
					if _floor_dist > _tolerance
					{
						is_pushing = false;
						IsGrounded = false;
						
						ani_upd_frame(0);
						break;
					}
				}
				
				if _floor_dist < -_MAX_TOLERANCE
				{
					break;
				}

				if SharedData.PlayerPhysics >= PHYSICS_S2
				{
					_floor_angle = sub_PlayerSnapFloorAngle(_floor_angle);
				}
				
				y -= _floor_dist;
				angle = _floor_angle;

			break;

			// Left wall
			case 3:
			
				var _floor_data = tile_find_2h(x - Radius.Y, y - Radius.X, x - Radius.Y, y + Radius.X, false, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if !stick_to_convex
				{
					var _tolerance = SharedData.PlayerPhysics < PHYSICS_S2 ? _MAX_TOLERANCE : min(_MIN_TOLERANCE + abs(floor(Speed.Y)), _MAX_TOLERANCE);
					if _floor_dist > _tolerance
					{
						is_pushing = false;
						IsGrounded = false;
						
						ani_upd_frame(0);
						break;
					}
				}
				
			    if _floor_dist < -_MAX_TOLERANCE
				{
					break;
				}

				if SharedData.PlayerPhysics >= PHYSICS_S2
				{
					_floor_angle = sub_PlayerSnapFloorAngle(_floor_angle);
				}
				
				x -= _floor_dist;
				angle = _floor_angle;

			break;
		}
	}

	private float SnapFloorAngle(float angle)
	{
		var _diff = abs(Angle % 180 - angle % 180);		
		if _diff > 45 && _diff < 135
		{
			angle = round(Angle / 90) % 4 * 90;
			if angle == 0
			{
				angle = 360;
			}
		}

		return angle;
	}

	private void ProcessSlopeRepel()
	{
		if !IsGrounded || stick_to_convex || Action == Actions.HammerRush
		{
			return;
		}
	
		if ground_lock_timer > 0
		{
			ground_lock_timer--;
		}
		else if abs(GroundSpeed) < 2.5
		{
			if SharedData.PlayerPhysics < PhysicsTypes.S3
			{
				if math_get_angle_quad(angle) != 0
				{	
					GroundSpeed = 0;	
					ground_lock_timer = 30;
					IsGrounded = false;
				} 
			}
			else if angle > 33.75 && angle <= 326.25
			{
				if angle > 67.5 && angle <= 292.5
				{
					IsGrounded = false;
				}
				else
				{
					GroundSpeed += angle < 180 ? -0.5 : 0.5;
				}
		
				ground_lock_timer = 30;
			}
		}
	}

	private void ProcessCollisionAir()
	{
		// Control routine checks
		if IsGrounded || is_dead
		{
			return;
		}
		
		// Action checks
		if Action == Actions.Glide || Action == ACTION_CLIMB
		{
			return;
		}
		
		var _wall_radius = radius_x_normal + 1;
		var _move_vector = math_get_vector_256(Speed.X, Speed.Y);
		var _move_quad = math_get_angle_quad(_move_vector);
		
		// Perform left wall collision if not moving mostly right
		if _move_quad != 1
		{
			var _wall_dist = tile_find_h(x - _wall_radius, y, false, TileLayer)[0];
			if _wall_dist < 0
			{
				x -= _wall_dist;
				Speed.X = 0;
				
				if _move_quad == 3
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if _move_quad != 3
		{
			var _wall_dist = tile_find_h(x + _wall_radius, y, true, TileLayer)[0];
			if _wall_dist < 0
			{
				x += _wall_dist;
				Speed.X = 0;
				
				if _move_quad == 1
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if _move_quad != 0
		{
			var _roof_data = tile_find_2v(x - Radius.X, y - Radius.Y, x + Radius.X, y - Radius.Y, false, TileLayer);
			var _roof_dist = _roof_data[0];
			var _roof_angle = _roof_data[1];
			
			if _move_quad == 3 && SharedData.PlayerPhysics >= PhysicsTypes.S3 && _roof_dist <= -14
			{
				// Perform right wall collision if moving mostly left and too far into the ceiling
				var _wall_dist = tile_find_h(x + _wall_radius, y, true, TileLayer)[0];
				if _wall_dist < 0
				{
					x += _wall_dist;
					Speed.X = 0;
					
					return;
				}
			}
			else if _roof_dist < 0
			{
				y -= _roof_dist;
				if _move_quad == 2 && math_get_angle_quad(_roof_angle) % 2 > 0 && Action != Actions.Flight
				{
					angle = _roof_angle;
					GroundSpeed = _roof_angle < 180 ? -Speed.Y : Speed.Y;
					Speed.Y = 0;
					
					player_land(id);
				}
				else
				{
					if Speed.Y < 0
					{
						Speed.Y = 0;
					}
						
					if Action == Actions.Flight
					{
						Gravity	= GravityType.TailsDown;
					}
				}
				
				return;
			}
		}
		
		// Perform floor collision if not moving mostly up
		if _move_quad != 2
		{
			var _floor_dist;
			var _floor_angle;

			if _move_quad == 0
			{
				var _floor_data_l = tile_find_v(x - Radius.X, y + Radius.Y, true, TileLayer);
				var _floor_data_r = tile_find_v(x + Radius.X, y + Radius.Y, true, TileLayer);

				if _floor_data_l[0] > _floor_data_r[0]
				{
					_floor_dist = _floor_data_r[0];
					_floor_angle = _floor_data_r[1];
				}
				else
				{
					_floor_dist = _floor_data_l[0];
					_floor_angle = _floor_data_l[1];
				}
					
				var _min_clip = -(Speed.Y + 8);		
				if _floor_dist >= 0 || _min_clip >= _floor_data_l[0] && _min_clip >= _floor_data_r[0]
				{
					return;
				}
					
				if math_get_angle_quad(_floor_angle) > 0
				{
					if Speed.Y > 15.75
					{
						Speed.Y = 15.75;
					}
						
					GroundSpeed = _floor_angle < 180 ? -Speed.Y : Speed.Y;
					Speed.X = 0;
				}
				else if _floor_angle > 22.5 && _floor_angle <= 337.5
				{
					GroundSpeed = _floor_angle < 180 ? -Speed.Y : Speed.Y;
					GroundSpeed /= 2;
				}
				else 
				{
					GroundSpeed = Speed.X;
					Speed.Y = 0;
				}
			}
			else if Speed.Y >= 0
			{
				var _floor_data = tile_find_2v(x - Radius.X, y + Radius.Y, x + Radius.X, y + Radius.Y, true, TileLayer);
				_floor_dist = _floor_data[0];
				_floor_angle = _floor_data[1];
							
				if _floor_dist >= 0
				{
					return;
				}
				
				GroundSpeed = Speed.X;
				Speed.Y = 0;
			}
			else
			{
				return;
			}

			y += _floor_dist;
			angle = _floor_angle;
			
			player_land(id);
		}
	}

	private void ProcessGlideCollision()
	{
		// This script is a modified copy of scr_player_collision_air()
		
		if Action != Actions.Glide
		{
			return;
		}
		
		var _wall_radius = radius_x_normal + 1;
		var _move_vector = math_get_vector_256(Speed.X, Speed.Y);
		var _move_quad = math_get_angle_quad(_move_vector);
		
		var _collision_flag_wall = false;
		var _collision_flag_floor = false;
		var _climb_y = y;
		
		// Perform left wall collision if not moving mostly right
		if _move_quad != 1
		{
			var _wall_dist = tile_find_h(x - _wall_radius, y, false, TileLayer)[0];
			if _wall_dist < 0
			{
				x -= _wall_dist;
				Speed.X = 0;
				_collision_flag_wall = true;
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if _move_quad != 3
		{
			var _wall_dist = tile_find_h(x + _wall_radius, y, true, TileLayer)[0];
			if _wall_dist < 0
			{
				x += _wall_dist;
				Speed.X = 0;
				_collision_flag_wall = true;
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if _move_quad != 0
		{
			var _roof_dist = tile_find_2v(x - Radius.X, y - Radius.Y, x + Radius.X, y - Radius.Y, false, TileLayer)[0];
			if _move_quad == 3 && _roof_dist <= -14 && SharedData.PlayerPhysics >= PhysicsTypes.S3
			{
				// Perform right wall collision instead if moving mostly left and too far into the ceiling
				var _wall_dist = tile_find_h(x + _wall_radius, y, true, TileLayer)[0];
				if _wall_dist < 0
				{
					x += _wall_dist;
					Speed.X = 0;
					_collision_flag_wall = true;
				}
			}
			else if _roof_dist < 0
			{
				y -= _roof_dist;
				if Speed.Y < 0 or _move_quad == 2
				{
					Speed.Y = 0;
				}
			}
		}
		
		// Perform floor collision if not moving mostly up
		if _move_quad != 2
		{
			var _floor_data = tile_find_2v(x - Radius.X, y + Radius.Y, x + Radius.X, y + Radius.Y, true, TileLayer);
			var _floor_dist = _floor_data[0];
			var _floor_angle = _floor_data[1];
		
			if ActionState == GLIDE_STATE_GROUND
			{
				if _floor_dist > 14
				{
					sub_PlayerGlideRelease();
				}
				else
				{
					y += _floor_dist;
					angle = _floor_angle;
				}
				
				return;
			}

			if _floor_dist < 0
			{
				y += _floor_dist;
				angle = _floor_angle;
				Speed.Y = 0;
				_collision_flag_floor = true;
			}
		}
		
		// Land logic
		if _collision_flag_floor
		{
			if ActionState == GLIDE_STATE_AIR
			{
				if math_get_angle_quad(angle) == 0
				{
					Animation = ANI_GLIDE_GROUND;
					ActionState = GLIDE_STATE_GROUND;
					ActionValue = 0;
					Gravity = 0;
				}
				else
				{
					GroundSpeed = angle < 180 ? Speed.X : -Speed.X;
					player_land(id);
				}
			}
			else if ActionState == GLIDE_STATE_FALL
			{
				player_land(id);
				audio_play_sfx(sfx_land);
				
				if math_get_angle_quad(angle) == 0
				{
					Animation = ANI_GLIDE_LAND;
					ground_lock_timer = 16;
					GroundSpeed = 0;
					Speed.X = 0;
				}
				else
				{
					GroundSpeed = Speed.X;
				}
			}
		}
		
		// Wall attach logic
		else if _collision_flag_wall
		{
			if ActionState != GLIDE_STATE_AIR
			{
				return;
			}
			
			// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he is either inside the ceiling or above the floor edge
			var _wall_dist = tile_find_h(x + _wall_radius * Facing, _climb_y - Radius.Y, Facing, TileLayer)[0];
			if _wall_dist != 0
			{
				// Cast a vertical sensor now. If the distance returned is negative, Knuckles is inside
				// the ceiling, else he is above the edge
				
				// Note: _find_mode is set to 2. LBR tiles are not ignored in this case
				var _floor_dist = tile_find_v(x + (_wall_radius + 1) * Facing, _climb_y - Radius.Y - 1, true, TileLayer, 2)[0];
				if _floor_dist < 0 || _floor_dist >= 12
				{
					sub_PlayerGlideRelease();
					return;
				}
				
				// Adjust Knuckles' Y position to place him just below the edge
				y += _floor_dist;
			}
			
			if Facing == FLIP_LEFT
			{
				x++;
			}
			
			Animation = ANI_CLIMB_WALL;
			Action = ACTION_CLIMB;
			ActionState = CLIMB_STATE_NORMAL;
			ActionValue = 0;
			GroundSpeed = 0;
			Speed.Y = 0;
			Gravity	= 0;
			
			audio_play_sfx(sfx_grab);
		}
	}

	private void ReleaseGlide()
	{
		Animation = ANI_GLIDE_FALL;
		ActionState = GLIDE_STATE_FALL;
		ActionValue = 0;
		Radius.X = radius_x_normal;
		Radius.Y = radius_y_normal;
		
		player_reset_gravity(id);
	}

	private void ProcessCarry()
	{
		if Type != PLAYER_TAILS || carry_timer > 0 && --carry_timer != 0
		{
			return;
		}
	
		if carry_target == noone
		{
			if Action != Actions.Flight 
			{
				return;
			}
		
			// Try to grab another player
			for (var p = 0; p < PLAYER_COUNT; p++)
			{
				if p == Id
				{
					continue;
				}
			
				var _player = player_get(p);
				if _player.Action == Actions.SpinDash || _player.Action == ACTION_CARRIED
				{
					continue;
				}
			
				var _x_dist = floor(_player.x - x);
				if _x_dist < -16 || _x_dist >= 16 
				{
					continue;
				}
			
				var _y_dist = floor(_player.y - y) - 32;
				if _y_dist < 0 || _y_dist >= 16
				{
					continue;
				}
			
				player_reset_state(_player);
				audio_play_sfx(sfx_grab);
			
				_player.Animation = ANI_GRAB;
				_player.Action = ACTION_CARRIED;
				carry_target = _player;
			
				with _player
				{
					sub_PlayerCarryAttachTo(other);
				}
			}
		}
		else
		{
			with carry_target
			{
				var _tails = other;
				var _previous_x = other.carry_target_x;
				var _previous_y = other.carry_target_y;	
			
				var InputPress = player_get_input(0);
				if InputPress.Abc
				{
					_tails.carry_target = noone;
					_tails.carry_timer = 18;
				
					IsSpinning = true;
					IsJumping = true;
					Action = Actions.None;
					Animation = Animations.Spin;
					Radius.X = radius_x_spin;
					Radius.Y = radius_y_spin;
					Speed.X = 0;
					Speed.Y = PhysicParams.MinimalJumpVelocity;
				
					var InputDown = player_get_input(1);
					if InputDown.Left
					{
						Speed.X = -2;
					}
					else if InputDown.Right
					{
						Speed.X = 2;
					}
				
					audio_play_sfx(sfx_jump);
				}
				else if _tails.Action != Actions.Flight || x != _previous_x || y != _previous_y
				{
					_tails.carry_target = noone;
					_tails.carry_timer = 60;
					Action = Actions.None;
				}
				else
				{
					sub_PlayerCarryAttachTo(_tails);
				}
			}
		}
	}

	private void AttachToPlayer(Player player)
	{
		Facing = _carrier.Facing;
		Speed.X = _carrier.Speed.X;
		Speed.Y = _carrier.Speed.Y;
		x = _carrier.x;
		y = _carrier.y + 28;
		image_xscale = _carrier.Facing;
		
		_carrier.carry_target_x = x;
		_carrier.carry_target_y = y;
	}

	#endregion

	#region UpdatePlayerSystems

	private void ProcessCamera()
	{
		if (IsDead) return;
		
		var camera = Camera.MainCamera;
		if (camera.Target != this) return;
	
		if (SharedData.CDCamera)
		{
			const int shiftDistanceX = 64;
			const int shiftSpeedX = 2;

			int shiftDirectionX = GroundSpeed != 0f ? Math.Sign(GroundSpeed) : (int)Facing;

			if (Math.Abs(GroundSpeed) >= 6 || Action == Actions.SpinDash)
			{
				if (camera.Delay.X == 0 && camera.BufferOffset.X != shiftDistanceX * shiftDirectionX)
				{
					camera.BufferOffset.X += shiftSpeedX * shiftDirectionX;
				}
			}
			else
			{
				camera.BufferOffset.X -= shiftSpeedX * Math.Sign(camera.BufferOffset.X);
			}
		}
	
		bool doShiftDown = Animation == Animations.Duck;
		bool doShiftUp = Animation == Animations.LookUp;
	
		if (doShiftDown || doShiftUp)
		{
			if (CameraViewTimer > 0)
			{
				CameraViewTimer--;
			}
		}
		else if (SharedData.SpinDash || SharedData.PeelOut)
		{
			CameraViewTimer = DefaultViewTime;
		}
	
		if (CameraViewTimer > 0)
		{
			if (camera.BufferOffset.Y != 0)
			{
				camera.BufferOffset.Y -= 2 * Math.Sign(camera.BufferOffset.Y);
			}
		}
		else
		{
			if (doShiftDown && camera.BufferOffset.Y < 88) 	
			{
				camera.BufferOffset.Y += 2;
			}
			else if (doShiftUp && camera.BufferOffset.Y > -104)
			{
				camera.BufferOffset.Y -= 2;
			}
		}
	}
	
	private void UpdateStatus()
	{
		if (IsDead) return;

		// TODO: find a better place for this (and make obj_dust_skid)
		if (Animation == Animations.Skid && AnimationTimer % 4 == 0)
		{
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
	
		if (InvincibilityFrames > 0)
		{
			Visible = (InvincibilityFrames-- & 4) >= 1 || InvincibilityFrames == 0;
		}
	
		if (ItemSpeedTimer > 0 && --ItemSpeedTimer == 0)
		{
			//TODO: audio
			//stage_reset_bgm();	
		}
	
		if (ItemInvincibilityTimer > 0 && --ItemInvincibilityTimer == 0)
		{
			//TODO: audio
			//stage_reset_bgm();
		}
	
		if (IsSuper)
		{
			if (Action == Actions.Transform)
			{
				if (--ActionValue == 0)
				{
					ObjectInteraction = true;
					Action = Actions.None;
				}
			}
		
			if (SuperValue == 0)
			{
				if (--RingCount <= 0)
				{
					RingCount = 0;
					InvincibilityFrames = 1;
					IsSuper = false;
					
					//TODO: audio
					//stage_reset_bgm();
				}
				else
				{
					SuperValue = 60;
				}
			}
			else
			{
				SuperValue--;
			}
		}
	
		IsInvincible = InvincibilityFrames != 0 || ItemInvincibilityTimer != 0 || IsHurt || IsSuper || Barrier.State == BARRIER_STATE_DOUBLESPIN;
				 
		if (Id == 0 && FrameworkData.Time >= 36000d)
		{
			Kill();
		}
	
		if (Id == 0 && LifeRewards.Length > 0)
		{
			if (RingCount >= LifeRewards[0] && LifeRewards[0] <= 200)
			{
				LifeCount++;
				LifeRewards[0] += 100;
					
				//TODO: audio
				//audio_play_sfx(sfx_extra_life);
			}

			if (ScoreCount < LifeRewards[1]) return;
			LifeCount++;
			LifeRewards[1] += 50000;
				
			//TODO: audio
			//audio_play_sfx(sfx_extra_life);
		}
		else
		{
			LifeRewards = [RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000];
		}
	}

	private void ProcessWater()
	{
		if is_dead || !instance_exists(c_stage) || !c_stage.water_enabled
		{
			return;
		}
		
		// On surface
		
		if !IsUnderwater
		{
			if floor(y) >= c_stage.water_level
			{	
				IsUnderwater = true;
				
				instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
				sub_PlayerWaterSplash();
				
				if !is_hurt
				{
					if !(Action == Actions.Flight || Action == Actions.Glide && ActionState != GLIDE_STATE_FALL)
					{
						Gravity = GRV_UNDERWATER;
					}
					
					Speed.X *= 0.5;
					Speed.Y *= 0.25;
				}
				
				if Barrier.Type == BARRIER_FLAME || Barrier.Type == BARRIER_THUNDER
				{	
					if Barrier.Type == BARRIER_THUNDER
					{
						instance_create(x, y, obj_water_flash);
					}
					
					Barrier.Type = Barrier.Types.None;			
				}
				
				if Action == Actions.Flight
				{
					audio_stop_sfx(sfx_flight);
					audio_stop_sfx(sfx_flight2);
				}
			}
			else
			{
				return;
			}
		}
		
		// Underwater
		
		if Barrier.Type != BARRIER_WATER
		{
			if air_timer > -1
			{
				air_timer--;
			}
			
			switch air_timer
			{
				case 1500: 
				case 1200:
				case 900:
						
					if Id == 0
					{
						audio_play_sfx(sfx_air_alert);
					}
						
				break;
					
				case 720:
					
					if Id == 0
					{
						audio_play_bgm(bgm_drowning);
					}
						
				break;
					
				case 0:
						
					audio_play_sfx(sfx_drown);
					ResetState();
					
					depth = 50;
					Animation = ANI_DROWN;
					TileLayer = TILELAYER_NONE;
					Speed.X = 0;
					Speed.Y = 0;
					Gravity	= 0.0625;
					IsAirLock = true;
					Camera.MainCamera.target = noone;
				
				return;
				
				case -1:
				
					if floor(y) > Camera.MainCamera.view_y + SharedData.game_height + 276
					{
						if Id == 0
						{
							FrameworkData.update_effects = false;
							FrameworkData.update_objects = false;
							FrameworkData.allow_pause = false;
						}
						
						is_dead = true;
					}
				
				return;
			}
		}
			
		if floor(y) < c_stage.water_level
		{
			if !is_hurt && Action != Actions.Glide
			{
				if SharedData.PlayerPhysics <= PHYSICS_S2 || Speed.Y >= -4
				{
					Speed.Y *= 2;
				}
					
				if Speed.Y < -16
				{
					Speed.Y = -16;
				}
					
				if Action != Actions.Flight
				{
					Gravity = GRV_DEFAULT;
				}
			}
				
			if Action == Actions.Flight
			{
				audio_play_sfx(sfx_flight, true);
			}
				
			if audio_is_playing(bgm_drowning)
			{
				stage_reset_bgm();
			}
				
			IsUnderwater = false;	
			air_timer = AIR_VALUE_MAX;
			
			sub_PlayerWaterSplash();
		}
	}

	private void ProcessWaterSplash()
	{
		if Action != ACTION_CLIMB && Action != Actions.Glide
		{
			instance_create(x, c_stage.water_level, obj_water_splash);
		}
				
		audio_play_sfx(sfx_water_splash);
	}

	private void UpdateCollision()
	{
		if is_dead
		{
			return;
		}
	
		if Animation != ANI_DUCK || SharedData.PlayerPhysics >= PhysicsTypes.S3
		{
			obj_set_hitbox(8, Radius.Y - 3);
		}
		else if Type != PLAYER_TAILS && Type != Types.Amy
		{
			obj_set_hitbox(8, 10, 0, 6);
		}
	
		// Clear extra hitbox
		if Animation != ANI_HAMMERRUSH && Animation != Animations.HammerSpin && Barrier.State != BARRIER_STATE_DOUBLESPIN
		{
			obj_set_hitbox_ext(0, 0);
		}
	
		obj_set_solid(radius_x_normal + 1, Radius.Y);
	}

	private void RecordData()
	{
		if is_dead
		{
			return;
		}
	
		ds_list_insert(ds_record_data, 0, [x, y, player_get_input(0), player_get_input(1), is_pushing, Facing]);
		ds_list_delete(ds_record_data, 32);
	}

	private void ProcessRotation()
	{
		if Animation != Animations.Move
		{
			visual_angle = 360;
		}
		else
		{
			if IsGrounded && SharedData.rotation_mode > 0
			{
				var _angle = 360;
				var _step = 0;
			
				if angle > 22.5 && angle <= 337.5
				{
					_angle = angle;
					_step = 2 - abs(GroundSpeed) * 3 / 32;
				}
				else
				{
					_step = 2 - abs(GroundSpeed) / 16;
				}
			
				visual_angle = darctan2(dsin(_angle) + dsin(visual_angle) * _step, dcos(_angle) + dcos(visual_angle) * _step);
			}
			else
			{
				visual_angle = angle;
			}
		}
	
		if SharedData.rotation_mode > 0
		{
			image_angle = visual_angle;
		}
		else
		{
			image_angle = ceil((visual_angle - 22.5) / 45) * 45;
		}
	}

	private void ProcessAnimate()
	{
		if FrameworkData.update_objects
		{
			if animation_buffer == -1 && AnimationTimer > 0
			{
				animation_buffer = Animation;
			}
		
			if AnimationTimer < 0
			{
				if Animation == animation_buffer
				{
					Animation = Animations.Move;
				}
			
				AnimationTimer = 0;
				animation_buffer = -1;
			}
			else if animation_buffer != -1
			{
				AnimationTimer--;
			}
		}
	
		if Animation != Animations.Spin || ani_get_timer() == ani_get_duration()
		{
			image_xscale = Facing;
		}
	
		switch Type
		{
			case Types.Sonic:
		
			if IsSuper
			{
				scr_player_animate_supersonic();
			}
			else
			{
				scr_player_animate_sonic();
			}
		
			break;
		
			case PLAYER_TAILS:
			scr_player_animate_tails();
			break;
		
			case PLAYER_KNUCKLES:
			scr_player_animate_knuckles();
			break;
		
			case Types.Amy:
			scr_player_animate_amy();
			break;
		}
	}
	
	private void ProcessPalette()
	{
		int[] colours = Type switch
		{
			Types.Tails => new[] { 4, 5, 6 },
			Types.Knuckles => new[] { 7, 8, 9 },
			Types.Amy => new[] { 10, 11, 12 },
			_ => new[] { 0, 1, 2, 3 }
		};

		// Get current active colour
		int colour = PaletteUtilities.Index[colours[0]];
	
		var colourLast = 0;
		var colourLoop = 0;
		var duration = 0;
	
		// Super State palette logic
		switch (Type)
		{
			case Types.Sonic:
				duration = colour switch
				{
					< 2 => 19,
					< 7 => 4,
					_ => 8
				};
			    
				colourLast = 16;
				colourLoop = 7;
				break;
			case Types.Tails:
				duration = colour < 2 ? 28 : 12;
				colourLast = 7;
				colourLoop = 2;
				break;
			case Types.Knuckles:
				duration = colour switch
				{
					< 2 => 17,
					< 3 => 15,
					_ => 3
				};

				colourLast = 11;
				colourLoop = 3;
				break;
			case Types.Amy:
				duration = colour < 2 ? 19 : 4;
				colourLast = 11;
				colourLoop = 3;
				break;
		}
	
		// Default palette logic (overwrites Super State palette logic)
		if (!IsSuper)
		{
			if (colour > 1)
			{
				if (Type == Types.Sonic)
				{
					colourLast = 21;
					duration = 4;
				}
			}
			else
			{
				colourLast = 1;
				duration = 0;
			}
		
			colourLoop = 1;
		}
	
		// Apply palette logic
		PaletteUtilities.SetRotation(colours, colourLoop, colourLast, duration);
	}

	#endregion
	
	public void SetInput(Buttons inputPress, Buttons inputDown)
	{
		InputPress = inputPress;
		InputDown = inputDown;
	}

	public void ResetGravity()
	{
		Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	}
    
	public void ResetState()
	{
		switch (Action)
		{
			//TODO: audio
			case Actions.PeelOut:
				//audio_stop_sfx(sfx_charge2);
				break;
		
			case Actions.Flight:
				//audio_stop_sfx(sfx_flight);
				//audio_stop_sfx(sfx_flight2);
				break;
		}
	
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		PushingObject = null;
		IsGrounded = false;
		OnObject = null;
	
		StickToConvex = false;
		GroundMode = Constants.GroundMode.Floor;
	
		Action = Actions.None;
	
		Radius = RadiusNormal;
	}
    
	private void EditModeInit()
	{
		EditModeObjects =
		[
			typeof(Common.Ring.Ring), typeof(Common.GiantRing.GiantRing), typeof(Common.ItemBox.ItemBox),
			typeof(Common.Springs.Spring), typeof(Common.Motobug.Motobug), typeof(Common.Signpost.Signpost)
		];
	    
		switch (FrameworkData.CurrentScene)
		{
			case Stages.TSZ.StageTSZ:
				// TODO: debug objects
				EditModeObjects.AddRange(new List<Type>
				{
					//typeof(obj_platform_swing_tsz), typeof(obj_platform_tsz), typeof(obj_falling_floor_tsz), typeof(obj_block_tsz)
				});
				break;
		}
	}

	private void UpdateInput()
	{
		if (Id >= InputUtilities.DeviceCount)
		{
			SetInput(new Buttons(), new Buttons());
			return;
		}
	    
		SetInput(InputUtilities.Press[Id], InputUtilities.Down[Id]);
	}

	private bool ProcessEditMode(float processSpeed)
	{
		if (Id > 0 || !(FrameworkData.PlayerEditMode || FrameworkData.DeveloperMode)) return false;

		bool debugButton;
		
		// If in developer mode, remap debug button to Spacebar
		if (FrameworkData.DeveloperMode)
		{
			debugButton = InputUtilities.DebugButtonPress;
			
			if (IsEditMode)
			{
				debugButton = debugButton || InputPress.B;
			}
		}
		else
		{
			debugButton = InputPress.B;
		}
		
		if (debugButton)
		{
			if (!IsEditMode)
			{
				if (FrameworkData.CurrentScene.IsStage)
				{
					//TODO: audio
					//stage_reset_bgm();
				}
				
				ResetGravity();
				ResetState();
				ResetZIndex();

				FrameworkData.UpdateGraphics = true;
				FrameworkData.UpdateObjects = true;
				FrameworkData.UpdateTimer = true;
				FrameworkData.AllowPause = true;
				
				ObjectInteraction = false;
				
				EditModeSpeed = 0;
				IsEditMode = true;
				
				Visible = true;
			}
			else
			{
				Speed = new Vector2();
				GroundSpeed = 0f;

				Animation = Animations.Move;
				
				ObjectInteraction = true;
				IsEditMode = false;
				IsDead = false;
			}
		}
		
		// Continue if Edit mode is enabled
		if (!IsEditMode) return false;

		// Update speed and position (move faster if in developer mode)
		if (InputDown.Up || InputDown.Down || InputDown.Left || InputDown.Right)
		{
			EditModeSpeed = MathF.Min(EditModeSpeed + (FrameworkData.DeveloperMode ? 
				EditModeAcceleration * EditModeAccelerationMultiplier : EditModeAcceleration), EditModeSpeedLimit);

			Vector2 position = Position;

			if (InputDown.Up)
			{
				position.Y -= EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Down)
			{
				position.Y += EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Left)
			{
				position.X -= EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Right)
			{
				position.X += EditModeSpeed * processSpeed;
			}

			Position = position;
		}
		else
		{
			EditModeSpeed = 0;
		}

		if (InputDown.A && InputPress.C)
		{
			if (--EditModeIndex < 0)
			{
				EditModeIndex = EditModeObjects.Count - 1;
			}
		}
		else if (InputPress.A)
		{
			if (++EditModeIndex >= EditModeObjects.Count)
			{
				EditModeIndex = 0;
			}
		}
		else if (InputPress.C)
		{
			if (Activator.CreateInstance(EditModeObjects[EditModeIndex]) is not Framework.CommonObject.CommonObject newObject) return true;
			newObject.Scale = new Vector2(newObject.Scale.X * (sbyte)Facing, newObject.Scale.Y);
			newObject.SetBehaviour(BehaviourType.Delete);
			FrameworkData.CurrentScene.AddChild(newObject);
		}
		
		return true;
	}

	private bool ProcessAI(float processSpeed)
	{
		if (Id == 0 || !FrameworkData.UpdateObjects) return false;

		// Find a player to follow
		CpuTarget ??= Players[Id - 1];
	
		if (Id < InputUtilities.DeviceCount && (InputDown.A || InputDown.B || InputDown.C || 
		    InputDown.Abc || InputDown.Up || InputDown.Down || InputDown.Left || InputDown.Right))
		{
			CpuInputTimer = 600f;
		}

		return true;

		//TODO: CpuState switch
		/*
	    switch (CpuState)
	    {
		    
	    }
	    */
	}

	private void RespawnAI()
	{
		if (!(Sprite == null || Sprite.CheckInView()))
		{
			if (++CpuTimer < 300) return;
			IsCpuRespawn = true;
			QueueFree();
		}
		else
		{
			CpuTimer = 0;
		}
	}

	private void ProcessPosition(float processSpeed)
	{
		if (Action == Actions.Carried) return;

		if (StickToConvex)
		{
			Speed = new Vector2(
				MathF.Clamp(Speed.X, -16f, 16f), 
				MathF.Clamp(Speed.Y, -16f, 16f));
		}

		Position += Speed * processSpeed;

		if (!IsGrounded)
		{
			Speed = new Vector2(Speed.X, Speed.Y + Gravity * processSpeed);
		}
	}

	private void ProcessRestart(float processSpeed)
	{
		if (!IsDead || Id > 0) return;

		bool isTimeOver = FrameworkData.Time >= 36000d; // TODO: add constant
		switch (RestartState)
		{
			// GameOver
			case RestartStates.GameOver:
				var bound = 32f;
		
				if (FrameworkData.PlayerPhysics < PhysicsTypes.S3)
				{
					bound += Camera.MainCamera.LimitBottom * processSpeed; // TODO: check if LimitBottom or Bounds
				}
				else
				{
					bound += Camera.MainCamera.BufferPosition.Y * processSpeed + SharedData.GameHeight;
				}
		
				if ((int)Position.Y > bound)
				{
					RestartState = RestartStates.ResetLevel;
					
					FrameworkData.AllowPause = false;
					FrameworkData.UpdateTimer = false;
					
					// TODO: Check this
					if (--LifeCount > 0 && !isTimeOver)
					{
						RestartTimer = 60f;
						break;
					}
					
					// TODO: audio + gameOver
					//instance_create_depth(0, 0, 0, obj_gui_gameover);				
					//audio_play_bgm(bgm_gameover);
				}
				break;
		
			// If RestartTimer was set
			case RestartStates.ResetLevel:
				if (RestartTimer > 0)
				{
					if (--RestartTimer != 0) break;
					RestartState = RestartStates.RestartStage;
				}
				// TODO: audio_bgm_is_playing
				// If restart_timer wasn't set (Game Over or Time Over)
				//else if (audio_bgm_is_playing()) break;
				RestartState = RestartStates.RestartGame;	
				
				// TODO: audio + fade
				//audio_stop_bgm(0.5);
				//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
				break;
		
			// Restart Stage
			case RestartStates.RestartStage:
				// TODO: Fade
				//if (FrameworkData.fade.state != Constants.FadeState.Max) break;

			    FrameworkData.SavedLives = LifeCount;
			    GetTree().ReloadCurrentScene();
				break;
		
			// Restart Game
			case RestartStates.RestartGame:
				// TODO: Game restart & fade
			    //if (FrameworkData.fade.state != Constants.FadeState.Max) break;
				
				//TODO: saved data
			    //FrameworkData.collected_giant_rings = [];
			    //FrameworkData.player_backup_data = [];
			    //FrameworkData.checkpoint_data = [];
				
			    // TODO: this
				//game_clear_temp_data();
			    if (SharedData.ContinueCount > 0)
			    {
				    //TODO: game_restart
				    //game_restart(); 
				    break;
			    }
				
				// Override save data
			    if (SharedData.CurrentSaveSlot != null)
			    {
				    SharedData.SavedLives = 3;
				    SharedData.SavedScore = 0;
				    SharedData.ContinueCount = 0;
					    
				    // game_save_data(SharedData.current_save_slot);
			    }
				
				//TODO: game_restart
			    //game_restart();
				break;
		}
	}
    
	public void Land()
	{
		if (!IsGrounded) return;

		ResetGravity();
	
		if (Action == Actions.Flight)
		{
			//TODO: audio
			//audio_stop_sfx(sfx_flight);
			//audio_stop_sfx(sfx_flight2);
		}
		else if (Action is Actions.SpinDash or Actions.PeelOut)
		{
			if (Action == Actions.PeelOut)
			{
				GroundSpeed = ActionValue2;
			}
		
			return;
		}
	
		if (BarrierFlag && Barrier.Type == Barrier.Types.Water)
		{
			float force = IsUnderwater ? -4f : -7.5f;
			Speed = new Vector2(MathF.Sin(MathF.DegToRad(Angle)), MathF.Sin(MathF.DegToRad(Angle))) * force;

			BarrierFlag = false;
			OnObject = null;
			IsGrounded = false;
		
			Barrier.UpdateFrame(0, 1, [3, 2]);
			Barrier.UpdateDuration([7, 12]);
			Barrier.Timer = 20d;
			
			//TODO: audio
			//audio_play_sfx(sfx_barrier_water2);
		
			return;
		}
	
		if (OnObject == null)
		{
			switch (Animation)
			{
				case Animations.Idle:
				case Animations.Duck:
				case Animations.HammerRush:
				case Animations.GlideGround: 
					break;
			
				default:
					Animation = Animations.Move;
					break;
			}
		}
		else
		{
			Animation = Animations.Move;
		}
	
		if (IsHurt)
		{
			InvincibilityFrames = 120;
			GroundSpeed = 0;
		}
	
		IsAirLock = false;
		IsSpinning	= false;
		IsJumping = false;
		PushingObject = null;
		IsHurt = false;
	
		BarrierFlag = false;
		ComboCounter = 0;
	
		CpuState = CpuStates.Main;

		DropDash();
		HammerSpin();
	
		if (Action != Actions.HammerRush)
		{
			Action = Actions.None;
		}
		else
		{
			GroundSpeed	= 6 * (sbyte)Facing;
		}

		if (IsSpinning) return;
		Position = new Vector2(Position.X, Position.Y - RadiusNormal.Y + Radius.Y);

		Radius = RadiusNormal;
	}

	public void Kill()
	{
		if (IsDead) return;

		Action = Actions.None;
		IsDead = true;
		ObjectInteraction = false;
		IsGrounded = false;
		OnObject = null;
		Barrier.Type = Barrier.Types.None;
		Animation = Animations.Death;
		Gravity = GravityType.Default;
		//y_vel = -7; // TODO: return variables
		//x_vel = 0;
		//ground_vel = 0;
		//Depth = 50; // TODO: Depth?
		
		// TODO: 
		if (Id == 0)
		{
			FrameworkData.UpdateObjects = false;
			FrameworkData.UpdateTimer = false;
			FrameworkData.AllowPause = false;
		}
		
		//TODO: Audio
		//audio_play_sfx(sfx_hurt);
	}

	private void DropDash()
	{
		if (!FrameworkData.DropDash || Action != Actions.DropDash) return;
	
		if (!IsGrounded)
		{
			ChargeDropDash();
		}
		else if (ActionValue == MaxDropDashCharge) // Called from player_land() function
		{
			ReleaseDropDash();
		}
	}
	
	private void ChargeDropDash()
	{
		if (InputDown.Abc)
		{
			IsAirLock = false;	
			if (ActionValue < MaxDropDashCharge)
			{
				ActionValue++;
			}
			else
			{
				if (Animation != Animations.DropDash)
				{
					Animation = Animations.DropDash;
					// TODO: audio
					//audio_play_sfx(sfx_charge);
				}
			}
		}
		else if (ActionValue > 0)
		{
			if (ActionValue == MaxDropDashCharge)
			{
				Animation = Animations.Spin;
				Action = Actions.DropDashCancel;
			}
			
			ActionValue = 0;
		}
	}
	
	private void ReleaseDropDash()
	{
		Position = new Vector2(Position.X, Position.Y + Radius.Y - RadiusSpin.Y);
		Radius = RadiusSpin;
		
		var force = 8f;
		var maxSpeed = 12f;
		
		if (IsSuper)
		{
			force = 12f;
			maxSpeed = 13f;
			Camera.MainCamera.UpdateShakeTimer(6);
		}
		
		if (Facing == Constants.Direction.Negative)
		{
			if (Angle <= 0)
			{
				GroundSpeed = MathF.Floor(GroundSpeed / 4f) - force;
				if (GroundSpeed < -maxSpeed)
				{
					GroundSpeed = -maxSpeed;
				}
			}
			else if (Angle != 360)
			{
				GroundSpeed = MathF.Floor(GroundSpeed / 2f) - force;
			}
			else
			{
				GroundSpeed = -force;
			}
		}
		else
		{
			if (Angle >= 0)
			{
				GroundSpeed = MathF.Floor(GroundSpeed / 4f) + force;
				if (GroundSpeed > maxSpeed)
				{
					GroundSpeed = maxSpeed;
				}
			}
			else if (Angle != 360)
			{
				GroundSpeed = MathF.Floor(GroundSpeed / 2f) + force;
			}
			else 
			{
				GroundSpeed = force;
			}
		}
		
		Animation = Animations.Spin;
		IsSpinning = true;
		
		if (!FrameworkData.CDCamera)
		{
			Camera.MainCamera.UpdateDelay(8);
		}
		
		Vector2 dustPosition = Position;
		dustPosition.Y += Radius.Y;
		Constants.Direction Facing = Facing;
		
		AddChild(new DropDashDust
		{
			Position = dustPosition,
			Scale = new Vector2((float)Facing, Scale.Y)
		});
		
		//TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}

	private void HammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
	
		const int maxHammerSpinCharge = 20;
	
		if (!InputDown.Abc)
		{
			if (ActionValue >= maxHammerSpinCharge)
			{
				Action = Actions.HammerSpinCancel;
			}
		
			ActionValue = 0;
			Animation = Animations.Spin;
		
			return;
		}

		if (ActionValue < maxHammerSpinCharge)
		{
			if (ActionValue == 0)
			{
				SetHitboxExtra(new Vector2I(25, 25));
				Animation = Animations.HammerSpin;
			}
		
			ActionValue++;
			if (ActionValue != maxHammerSpinCharge) return;
			
			// TODO: audio
			//audio_play_sfx(sfx_charge);
		}
	
		// Called from player_land() function
		if (!IsGrounded) return;
		
		Animation = Animations.HammerRush;
		Action = Actions.HammerRush;
		ActionValue = 59; // (60)
	
		// TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}
}
