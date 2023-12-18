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
	
	public enum ClimbStates : byte
	{
		Normal, Ledge
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

	public Buttons InputPress;
	public Buttons InputDown;

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

		Position += new Vector2(0f, 1f - Radius.Y);

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
		
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
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
		
			if (!Mathf.IsEqualApprox(ActionValue, 30f))
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
			Constants.GroundMode.Floor => CollisionUtilities.FindTileTwoPositions(true, 
				position - Radius, 
				position + new Vector2I(Radius.X, -Radius.Y), 
				Constants.Direction.Negative, TileLayer, GroundMode).Item1,
			Constants.GroundMode.RightWall => CollisionUtilities.FindTileTwoPositions(true, 
				position - new Vector2I(Radius.Y, Radius.X), 
				position + new Vector2I(-Radius.Y, Radius.X), 
				Constants.Direction.Negative, TileLayer, GroundMode).Item1,
			Constants.GroundMode.LeftWall => CollisionUtilities.FindTileTwoPositions(true, 
				x + Radius.Y, y - Radius.X, 
				x + Radius.Y, y + Radius.X, 
				Constants.Direction.Positive, TileLayer, GroundMode).Item1,
			_ => maxCeilingDist
		};
	
		if (ceilDist < maxCeilingDist) return false;
	
		// ???
		if (!SharedData.FixJumpSize)
		{
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
		Speed += PhysicParams.JumpVelocity * new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
		
		IsSpinning = true;	
		IsJumping = true;
		PushingObject = null;
		IsGrounded = false;
		OnObject = null;
		StickToConvex = false;
		GroundMode = 0;
	
		if (Type == Types.Amy)
		{
			SetHitboxExtra(new Vector2I(25, 25));
			Animation = Animations.HammerSpin;
		}
		else
		{
			Animation = Animations.Spin;
		}
		
		//TODO: audio
		//audio_play_sfx(sfx_jump);
	
		// return player control routine
		return true;
	}

	private void ProcessDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return;
	
		const int maxDropDashCharge = 20;
	
		if (!IsGrounded)
		{		
			if (InputDown.Abc)
			{
				IsAirLock = false;		
				if (ActionValue < maxDropDashCharge)
				{
					ActionValue++;
				}
				else 
				{
					if (Animation != Animations.DropDash)
					{
						Animation = Animations.DropDash;
						//TODO: audio
						//audio_play_sfx(sfx_charge);
					}
				}
			}
			else if (ActionValue > 0)
			{
				if (Mathf.IsEqualApprox(ActionValue, maxDropDashCharge))
				{		
					Animation = Animations.Spin;
					Action = Actions.DropDashCancel;
				}
			
				ActionValue = 0;
			}
		}
	
		// Called from player_land() function
		else if (Mathf.IsEqualApprox(ActionValue, maxDropDashCharge))
		{
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;
		
			var force = 8;
			var maxSpeed = 12;
		
			if (IsSuper)
			{
				force = 12;
				maxSpeed = 13;
				Camera.MainCamera.ShakeTimer = 6;
			}
		
			if (Facing == Constants.Direction.Negative)
			{
				if (Speed.X <= 0)
				{
					GroundSpeed = Mathf.Floor(GroundSpeed / 4f) - force;
					if (GroundSpeed < -maxSpeed)
					{
						GroundSpeed = -maxSpeed;
					}
				}
				else if (!Mathf.IsEqualApprox(Angle, 360f)) 
				{
					GroundSpeed = Mathf.Floor(GroundSpeed / 2f) - force;
				}
				else
				{
					GroundSpeed = -force;
				}
			}
			else 
			{
				if (Speed.X >= 0f)
				{
					GroundSpeed = Mathf.Floor(GroundSpeed / 4f) + force;
					if (GroundSpeed > maxSpeed)
					{
						GroundSpeed = maxSpeed;
					}
				}
				else if (!Mathf.IsEqualApprox(Angle, 360f))
				{
					GroundSpeed = Mathf.Floor(GroundSpeed / 2f) + force;
				}
				else 
				{
					GroundSpeed = force;
				}
			}
		
			Animation = Animations.Spin;
			IsSpinning = true;
		
			if (!SharedData.CDCamera)
			{
				Camera.MainCamera.Delay.X = 8;
			}
			
			//TODO: audio & obj_dust_dropdash
			//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
			//audio_stop_sfx(sfx_charge);
			//audio_play_sfx(sfx_release);
		}
	}
	
	private void ProcessFlight()
	{
		if (Action != Actions.Flight) return;
	
		if (ActionValue > 0f)
		{
			if (--ActionValue == 0f)
			{
				if (!IsUnderwater)
				{
					Animation = Animations.FlyTired;
					//TODO: audio
					//audio_play_sfx(sfx_flight2, true);
				}
				else
				{
					Animation = Animations.SwimTired;
				}
			
				Gravity = GravityType.TailsDown;
				//TODO: audio
				//audio_stop_sfx(sfx_flight);
			}
			else
			{	
				if (!IsUnderwater)
				{
					Animation = CarryTarget == null ? Animations.Fly : Animations.FlyLift;
				}
				else
				{
					Animation = CarryTarget == null ? Animations.Swim : Animations.SwimLift;
				}
			
				if (!IsUnderwater || CarryTarget == null)
				{
					if (InputPress.Abc)
					{
						Gravity = GravityType.TailsUp;
					}
					else if (Speed.Y < -1)
					{
						Gravity = GravityType.TailsDown;
					}
				
					Speed.Y = Math.Max(Speed.Y, -4);
				}
			}
		}

		if (!SharedData.FlightCancel || !InputDown.Down || !InputPress.Abc) return;
		
		Camera.MainCamera.BufferPosition.Y += Radius.Y - RadiusSpin.Y;
		Radius= RadiusSpin;
		Animation = Animations.Spin;
		IsSpinning	= true;
		Action = Actions.None;
		
		//audio_stop_sfx(sfx_flight);
		//audio_stop_sfx(sfx_flight2);
		ResetGravity();
	}
	
	private void ProcessClimb()
	{
		if (Action != Actions.Climb) return;
		
		switch ((ClimbStates)ActionState)
		{
			case ClimbStates.Normal:
				if (!Mathf.IsEqualApprox(Position.X, PreviousPosition.X))
				{
					ReleaseClimb();
					break;
				}
		
				const int stepsPerFrame = 4;
				var maxValue = image_number * stepsPerFrame;
				
				if (InputDown.Up)
				{
					if (++ActionValue > maxValue)
					{
						ActionValue = 0;
					}
					
					Speed.Y = -PhysicParams.AccelerationClimb;
				}
				else if (InputDown.Down)
				{
					if (--ActionValue < 0)
					{
						ActionValue = maxValue;
					}
					
					Speed.Y = PhysicParams.AccelerationClimb;
				}
				else
				{
					Speed.Y = 0;
				}
				
				int radiusX = Radius.X;
				if (Facing == Constants.Direction.Negative)
				{
					radiusX++;
				}
		
				if (Speed.Y < 0)
				{
					// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
					Vector2I position = (Vector2I)Position + new Vector2I(radiusX * (int)Facing, -Radius.Y - 1);
					(sbyte wallDistance, float? _) = CollisionUtilities.FindTile(false, position, Facing, TileLayer, );
					if (wallDistance >= 4)
					{
						ActionState = (int)ClimbStates.Ledge;
						ActionValue = 0;
						Speed.Y = 0;
						
						break;
					}
		
					// If Knuckles has encountered a small dip in the wall, cancel climb movement
					if (wallDistance != 0)
					{
						Speed.Y = 0;
					}
		
					// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
					var ceilDist = tile_find_v(x + radiusX * Facing, y - RadiusNormal.Y + 1, false, TileLayer)[0];
					if (ceilDist < 0)
					{
						Position.Y -= ceilDist;
						Speed.Y = 0;
					}
				}
				else
				{
					// If Knuckles is no longer against the wall, make him let go
					var wallDist = tile_find_h(x + radiusX * Facing, y + Radius.Y + 1, Facing, TileLayer)[0];
					if (wallDist != 0)
					{
						ReleaseClimb();
						break;
					}
					
					// If Knuckles has reached the floor, make him land
					var floorData = tile_find_v(x + radiusX * Facing, y + RadiusNormal.Y, true, TileLayer);
					var floorDist = floorData[0];
					var floorAngle = floorData[1];
					
					if (floorDist < 0)
					{
						Position += new Vector2(0f, floorDist + RadiusNormal.Y - Radius.Y);
						Angle = floorAngle;
						
						Land();

						Animation = Animations.Idle;
						Speed.Y = 0;
						
						break;
					}
				}
				
				if (InputPress.Abc)
				{
					Animation = Animations.Spin;
					IsSpinning = true;
					IsJumping = true;		
					Action = Actions.None;
					Facing = (Constants.Direction)(-(int)Facing);
					Speed = new Vector2(3.5f * (float)Facing, PhysicParams.MinimalJumpVelocity);
					
					//TODO: audio
					//audio_play_sfx(sfx_jump);
					ResetGravity();
				}
				else if (Speed.Y != 0)
				{
					Sprite.UpdateFrame(Mathf.FloorToInt(ActionValue / stepsPerFrame));
				}
				break;
			
			case ClimbStates.Ledge:
				switch (ActionValue++)
				{
					case 0: // Frame 0
						Animation = Animations.ClimbLedge;
						Position += new Vector2(3f * (float)Facing, -2f);
						break;
					
					case 6: // Frame 1
						Position += new Vector2(8f * (float)Facing, -10f);
						break;
					
					case 12: // Frame 2
						Position -= new Vector2(8f * (float)Facing, 12f);
						break;
					
					case 18: // End
						Land();
						Animation = Animations.Idle;
						Position += new Vector2(8f * (float)Facing, 4f);
						break;
				}
				break;
		}
	}

	private void ReleaseClimb()
	{
		Animation = Animations.GlideFall;
		Action = Actions.Glide;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 1;
		Radius = RadiusNormal;
		
		ResetGravity();
	}
	
	private void ProcessGlide()
	{
		if (Action != Actions.Glide || (GlideStates)ActionState == GlideStates.Fall) return;
		
		const float angleIncrement = 2.8125f;
		const float glideGravity = 0.125f;
		const float slideFriction = 0.09375f;
		
		switch ((GlideStates)ActionState)
		{
			case GlideStates.Air:
				//TODO: check this
				// ground vel - glide speed
				// ActionValue - glide Angle
				
				// Update glide speed
				if (GroundSpeed >= 4f)
				{
					if (ActionValue % 180f == 0)
					{
						GroundSpeed = Math.Min(GroundSpeed + PhysicParams.AccelerationGlide, 24f);
					}
				}
				else
				{
					GroundSpeed += 0.03125f;
				}
				
				// Turn around
				if (InputDown.Left && !Mathf.IsZeroApprox(ActionValue))
				{
					if (ActionValue > 0f)
					{
						ActionValue = -ActionValue;
					}
					
					ActionValue += angleIncrement;
				}
				else if (InputDown.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
				{
					if (ActionValue < 0f)
					{
						ActionValue = -ActionValue;
					}
					
					ActionValue += angleIncrement;
				}
				else if (!Mathf.IsZeroApprox(ActionValue % 180f))
				{
					ActionValue += angleIncrement;
				}
				
				// Update horizontal and vertical speed
				Speed.X = GroundSpeed * -Mathf.Cos(Mathf.DegToRad(ActionValue));
				if (Speed.Y < 0.5f)
				{
					Gravity = glideGravity;
				}
				else
				{
					Gravity = -glideGravity;
				}
				
				// Update Animation frame
				float angle = Math.Abs(ActionValue) % 180f;
				switch (angle)
				{
					case < 30f or > 150f:
						Sprite.UpdateFrame(0);
						break;
					case < 60f or > 120f:
						Sprite.UpdateFrame(1);
						break;
					default:
						Facing = angle < 90 ? Constants.Direction.Negative : Constants.Direction.Positive;
						Sprite.UpdateFrame(2);
						break;
				}
				
				if (!InputDown.Abc)
				{
					Animation = Animations.GlideFall;
					ActionState = (int)GlideStates.Fall;
					ActionValue = 0f;
					Radius = RadiusNormal;
					Speed.X *= 0.25f;

					ResetGravity();
				}
				break;
			
			case GlideStates.Ground:
				if (!InputDown.Abc)
				{
					Speed.X = 0;
				}
				else if (Speed.X > 0)
				{
					Speed.X = Math.Max(0f, Speed.X - slideFriction);
				}
				else if (Speed.X < 0)
				{
					Speed.X = Math.Min(0f, Speed.X + slideFriction);
				}

				if (Speed.X == 0)
				{
					Land();
					Sprite.UpdateFrame(1);

					Animation = Animations.GlideGround;
					GroundLockTimer = 16;
					GroundSpeed = 0;

					break;
				}

				if (ActionValue % 4 == 0)
				{
					//TODO: obj_dust_skid
					//instance_create(x, y + Radius.Y, obj_dust_skid);
				}
				
				if (ActionValue > 0 && ActionValue % 8 == 0)
				{
					//TODO: audio
					//audio_play_sfx(sfx_slide);
				}
					
				ActionValue++;
				break;
			
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	private void ProcessHammerSpin()
	{
		if (Action != Actions.HammerSpin) return;
	
		var maxHammerSpinCharge = 20;
	
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
			if (!Mathf.IsEqualApprox(ActionValue, maxHammerSpinCharge))
			{
				return;
			}
			
			//TODO: audio
			//audio_play_sfx(sfx_charge);
		}
		
		// Called from player_land() function
		if (!IsGrounded)
		{
			return;
		}
	
		Animation = Animations.HammerRush;
		Action = Actions.HammerRush;
		ActionValue = 59; // (60)
			
		//TODO: audio
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}
	
	private void ProcessHammerRush()
	{
		if (Action != Actions.HammerRush) return;

		var sign = (int)Facing;
		(Vector2I radius, Vector2I offset) = (Sprite.Frame & 3) switch
		{
			0 => (new Vector2I(16, 16), new Vector2I(6 * sign, 0)),
			1 => (new Vector2I(16, 16), new Vector2I(-7 * sign, 0)),
			2 => (new Vector2I(14, 20), new Vector2I(-4 * sign, -4)),
			3 => (new Vector2I(17, 21), new Vector2I(7 * sign, -5)),
			_ => throw new ArgumentOutOfRangeException()
		};
		SetHitboxExtra(radius, offset);

		if (IsGrounded)
		{
			float radians = Mathf.DegToRad(Angle);
			if (!InputDown.Abc || ActionValue == 0 || GroundSpeed == 0 || Math.Cos(radians) <= 0)
			{
				CancelHammerRush();
				return;
			}
		
			ActionValue--;
		
			if (InputDown.Left && GroundSpeed > 0 || InputDown.Right && GroundSpeed < 0)
			{
				Facing = (Constants.Direction)(-(int)Facing);
				GroundSpeed *= -1;
			}

			radians = Mathf.DegToRad(Angle);
			Speed = GroundSpeed * new Vector2(MathF.Cos(radians), MathF.Sin(radians));
		}
		else if (Speed.X == 0f)
		{
			CancelHammerRush();
		}
		else
		{
			Speed.X = 6f * Math.Sign(GroundSpeed);
		}
	}

	private void CancelHammerRush()
	{
		Animation = Animations.Move;
		Action = Actions.None;
	}

	private void ProcessSlopeResist()
	{
		if (!IsGrounded || IsSpinning || Angle is > 135f and <= 225f) return;
	
		if (Action is Actions.HammerRush or Actions.PeelOut) return;

		float radians = Mathf.DegToRad(Angle);
		float slopeGrv = 0.125f * MathF.Sin(radians);
		if (GroundSpeed != 0f || SharedData.PlayerPhysics >= PhysicsTypes.S3 && Math.Abs(slopeGrv) > 0.05078125f)
		{
			GroundSpeed -= slopeGrv;
		}
	}

	private void ProcessSlopeResistRoll()
	{
		if (!IsGrounded || !IsSpinning || Angle is > 135 and <= 225) return;
	
		float radians = Mathf.DegToRad(Angle);
		float slopeGrv = Math.Sign(GroundSpeed) != Math.Sign(MathF.Sin(radians)) ? 0.3125f : 0.078125f;
		GroundSpeed -= slopeGrv * MathF.Sin(radians);
	}

	private void ProcessMovementGround()
	{
		// Control routine checks
		if (!IsGrounded || IsSpinning) return;
		
		// Action checks
		if (Action is Actions.SpinDash or Actions.PeelOut or Actions.HammerRush) return;
		
		var doSkid = false;
		
		// If Knuckles is standing up from a slide and DOWN button is pressed, cancel
		// control lock. This allows him to Spin Dash
		if (Animation == Animations.GlideGround && InputDown.Down)
		{
			GroundLockTimer = 0f;
		}
		
		if (Mathf.IsZeroApprox(GroundLockTimer))
		{
			// Move left
			if (InputDown.Left)
			{	
				if (GroundSpeed > 0f)
				{
					GroundSpeed -= PhysicParams.Deceleration;
					if (GroundSpeed <= 0f)
					{
						GroundSpeed = -0.5f;
					}
					
					doSkid = true;
				}
				else
				{
					if (!SharedData.NoSpeedCap || GroundSpeed > -PhysicParams.AccelerationTop)
					{
						GroundSpeed = Math.Max(GroundSpeed - PhysicParams.Acceleration, -PhysicParams.AccelerationTop);
					}
					
					if (Facing != Constants.Direction.Negative)
					{
						Animation = Animations.Move;
						Facing = Constants.Direction.Negative;
						PushingObject = null;
						
						Sprite.UpdateFrame(0);
					}
				}
			}
			
			// Move right
			if (InputDown.Right)
			{
				if (GroundSpeed < 0f)
				{
					GroundSpeed += PhysicParams.Deceleration;
					if (GroundSpeed >= 0f)
					{
						GroundSpeed = 0.5f;
					}
					
					doSkid = true;
				} 
				else
				{
					if (!SharedData.NoSpeedCap || GroundSpeed < PhysicParams.AccelerationTop)
					{
						GroundSpeed = Math.Min(GroundSpeed + PhysicParams.Acceleration, PhysicParams.AccelerationTop);
					}
					
					if (Facing != Constants.Direction.Positive)
					{
						Animation = Animations.Move;
						Facing = Constants.Direction.Positive;
						PushingObject = null;
						
						Sprite.UpdateFrame(0);;
					}
				}
			}
			
			// Set idle / move / skid Animation
			if (MathB.GetAngleQuadrant(Angle) == 0f)
			{
				if (doSkid && Math.Abs(GroundSpeed) >= 4f && Animation != Animations.Skid)
				{
					AnimationTimer = Type == Types.Sonic ? 24f : 16f;
					Animation = Animations.Skid;
					
					//TODO: audio
					//audio_play_sfx(sfx_skid);
				}
				
				if (GroundSpeed == 0)
				{
					if (InputDown.Up)
					{
						Animation = Animations.LookUp;
					}
					else if (InputDown.Down)
					{
						Animation = Animations.Duck;
					}
					else
					{
						Animation = Animations.Idle;
					}
					
					PushingObject = null;
				}
				
				// TODO: This
				else if (Animation != Animations.Skid && Animation != Animations.Push)
				{
					Animation = Animations.Move;
				}
			}
			
			else if (Animation != Animations.Skid && Animation != Animations.Push)
			{
				Animation = Animations.Move;
			}
			
			// Set / clear push Animation
			if (PushingObject != null)
			{
				if (Animation == Animations.Move && Mathf.IsEqualApprox(Sprite.GetTimer(), Sprite.GetDuration()))
				{
					Animation = Animations.Push;
				}
			}
			else if (Animation == Animations.Push)
			{
				Animation = Animations.Move;
			}
		}
		
		// Apply friction
		if (InputDown is { Left: false, Right: false })
		{
			GroundSpeed = GroundSpeed switch
			{
				> 0f => Math.Max(GroundSpeed - PhysicParams.Friction, 0f),
				< 0f => Math.Min(GroundSpeed + PhysicParams.Friction, 0f),
				_ => GroundSpeed
			};
		}
		
		// Convert ground velocity into directional velocity
		float radians = Mathf.DegToRad(Angle);
		Speed = GroundSpeed * new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
	}

	private void ProcessMovementGroundRoll()
	{
		// Control routine checks
		if (!IsGrounded || !IsSpinning) return;

		if (GroundLockTimer == 0f)
		{
			// Move left
			if (InputDown.Left)
			{
				if (GroundSpeed > 0)
				{
					
					GroundSpeed -= PhysicParams.DecelerationRoll;
					if (GroundSpeed < 0)
					{
						GroundSpeed = -0.5f;
					}
				}
				else
				{
					Facing = Constants.Direction.Negative;
					PushingObject = null;	
				}
			}
		
			// Move right
			if (InputDown.Right)
			{
				if (GroundSpeed < 0)
				{
					GroundSpeed += PhysicParams.DecelerationRoll;
					if (GroundSpeed >= 0)
					{
						GroundSpeed = 0.5f;
					}
				}
				else
				{
					Facing = Constants.Direction.Positive;
					PushingObject = null;	
				}
			}
		}
	
		// Apply friction
		GroundSpeed = GroundSpeed switch
		{
			> 0 => Math.Max(GroundSpeed - PhysicParams.FrictionRoll, 0),
			< 0 => Math.Min(GroundSpeed + PhysicParams.FrictionRoll, 0),
			_ => GroundSpeed
		};

		// Stop spinning
		if (!IsForcedRoll)
		{
			if (GroundSpeed == 0f || SharedData.PlayerPhysics == PhysicsTypes.SK && Math.Abs(GroundSpeed) < 0.5f)
			{
				Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);

				Radius = RadiusNormal;
			
				IsSpinning = false;
				Animation = Animations.Idle;
			}
		}
	
		// If forced to spin, keep moving player
		else
		{
			if (SharedData.PlayerPhysics == PhysicsTypes.CD)
			{
				if (GroundSpeed is >= 0f and < 2f)
				{
					GroundSpeed = 2f;
				}
			}
			else if (GroundSpeed == 0f)
			{
				GroundSpeed = SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f * (float)Facing;
			}
		}
	
		float radians = Mathf.DegToRad(Angle);
		Speed = GroundSpeed * new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
		Speed.X = Math.Clamp(Speed.X, -16f, 16f);
	}

	private void ProcessMovementAir()
	{
		// Control routine checks
		if (IsGrounded || IsDead)
		{
			return;
		}
	
		// Action checks
		if (Action is Actions.Carried or Actions.Climb or Actions.Glide
		    && (GlideStates)ActionState != GlideStates.Fall) return;
	
		// Update Angle (rotate player)
		if (!Mathf.IsEqualApprox(Angle, 360f))
		{
			if (Angle >= 180f)
			{
				Angle += 2.8125f;
			}
			else
			{
				Angle -= 2.8125f;
			}
		
			if (Angle is <= 0f or > 360f)
			{
				Angle = 360f;
			}
		}
	
		// Limit upward speed
		if (!IsJumping && Action != Actions.SpinDash && !IsForcedRoll)
		{
			if (Speed.Y < -15.75f)
			{
				Speed.Y = -15.75f;
			}
		}
	
		// Limit downward speed
		if (SharedData.PlayerPhysics == PhysicsTypes.CD && Speed.Y > 16)
		{
			Speed.Y = 16;
		}
	
		if (!IsAirLock)
		{
			// Move left
			if (InputDown.Left)
			{
				if (Speed.X > 0) 
				{
					Speed.X -= PhysicParams.AccelerationAir;
				}
				else if (!SharedData.NoSpeedCap || Speed.X > -PhysicParams.AccelerationTop)
				{
					Speed.X = Math.Max(Speed.X - PhysicParams.AccelerationAir, -PhysicParams.AccelerationTop);
				}
			
				Facing = Constants.Direction.Negative;
			}
		
			// Move right
			if (InputDown.Right)
			{
				if (Speed.X < 0)
				{
					Speed.X += PhysicParams.AccelerationAir;
				} 
				else if (!SharedData.NoSpeedCap || Speed.X < PhysicParams.AccelerationTop)
				{
					Speed.X = Math.Min(Speed.X + PhysicParams.AccelerationAir, PhysicParams.AccelerationTop);
				}
			
				Facing = Constants.Direction.Positive;
			}	
		}
	
		// Apply air drag
		if (!IsHurt && Speed.Y is < 0f and > -4f)
		{
			Speed.X -= Mathf.Floor(Speed.X * 8f) / 256f;
		}
	}

	private void ProcessBalance()
	{
		if (!IsGrounded || IsSpinning) return;

		if (GroundSpeed != 0 || Action is Actions.SpinDash or Actions.PeelOut) return;
		
		if (SharedData.PlayerPhysics == PhysicsTypes.SK && InputDown.Down || InputDown.Up && SharedData.PeelOut) return;
	
		if (OnObject == null)
		{
			if (MathB.GetAngleQuadrant(Angle) > 0) return;
		
			var floorDist = tile_find_v(x, y + Radius.Y, true, TileLayer)[0];	
			if (floorDist < 12) return;
		
			var angleLeft = tile_find_v(x - Radius.X, y + Radius.Y, true, TileLayer)[1];
			var angleRight = tile_find_v(x + Radius.X, y + Radius.Y, true, TileLayer)[1];
		
			if (angleLeft != null && angleRight != null) return;
		
			if (angleLeft == null)
			{	
				var leftDist = tile_find_v(x + 6, y + Radius.Y, true, TileLayer)[0];
				BalanceLeft(leftDist >= 12);
			}
			else
			{
				var rightDist = tile_find_v(x - 6, y + Radius.Y, true, TileLayer)[0];
				BalanceRight(rightDist >= 12);
			}
		}
		else if (IsInstanceValid(OnObject)) // TODO: check IsInstanceValid == instance_exist
		{
			if (OnObject.SolidData.NoBalance)
			{
				return;
			}
		
			const int leftEdge = 2;
			int rightEdge = OnObject.SolidData.Radius.X * 2 - leftEdge;
			int playerX = Mathf.FloorToInt(OnObject.SolidData.Radius.X - OnObject.Position.X + Position.X);
			
			if (playerX < leftEdge)
			{
				BalanceLeft(playerX < leftEdge - 4);
			}
			else if (playerX > rightEdge)
			{
				BalanceRight(playerX > rightEdge + 4);
			}
		}
	}

	private void BalanceLeft(bool isPanic)
	{
		if (Type != Types.Sonic || IsSuper)
		{
			Animation = Animations.Balance;
			Facing = Constants.Direction.Negative;
			
			return;
		}
		
		if (!isPanic)
		{
			Animation = Facing == Constants.Direction.Negative ? Animations.Balance : Animations.BalanceFlip;
		}
		else if (Facing == Constants.Direction.Positive)
		{
			Animation = Animations.BalanceTurn;
			Facing = Constants.Direction.Negative;
		}
		else if (Animation != Animations.BalanceTurn)
		{
			Animation = Animations.BalancePanic;
		}
	}

	private void BalanceRight(bool isPanic)
	{
		if (Type != Types.Sonic || IsSuper)
		{
			Animation = Animations.Balance;
			Facing = Constants.Direction.Positive;
			
			return;
		}
		
		if (!isPanic)
		{
			Animation = (Facing == Constants.Direction.Positive) ? Animations.Balance : Animations.BalanceFlip;
		}
		else if (Facing == Constants.Direction.Negative)
		{
			Animation = Animations.BalanceTurn;
			Facing = Constants.Direction.Positive;
		}
		else if (Animation != Animations.BalanceTurn)
		{
			Animation = Animations.BalancePanic;
		}
	}

	private void ProcessCollisionGroundWalls()
	{
		// Control routine checks
		if (!IsGrounded)
		{
			return;
		}
		
		if (SharedData.PlayerPhysics < PhysicsTypes.SK)
		{
			if (Angle is > 90f and <= 270f) return;
		}
		else if (Angle is >= 90f and <= 270f && Angle % 90f != 0f)
		{
			return;
		}

		int castDir = Angle switch
		{
			>= 45 and <= 128 => 1,
			> 128 and < 225 => 2,
			>= 225 and < 315 => 3,
			_ => 0
		};

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = 8 * (Angle == 360 ? 1 : 0);
		
		if (GroundSpeed < 0)
		{
			var wallDist = castDir switch
			{
				0 => tile_find_h(x + Speed.X - wallRadius, y + Speed.Y + offsetY, false, TileLayer, GroundMode)[0],
				1 => tile_find_v(x + Speed.X, y + Speed.Y + wallRadius, true, TileLayer, GroundMode)[0],
				2 => tile_find_h(x + Speed.X + wallRadius, y + Speed.Y, true, TileLayer, GroundMode)[0],
				3 => tile_find_v(x + Speed.X, y + Speed.Y - wallRadius, false, TileLayer, GroundMode)[0],
				_ => 0
			};

			if (wallDist >= 0) return;
			
			switch (MathB.GetAngleQuadrant(Angle))
			{
				case 0:
					Speed.X -= wallDist;
					GroundSpeed = 0;
						
					if (Facing == Constants.Direction.Negative && !IsSpinning)
					{
						PushingObject = this;
					}
					break;
					
				case 1:
					Speed.Y += wallDist;
					break;
					
				case 2:
					Speed.X += wallDist;
					GroundSpeed = 0;
						
					if (Facing == Constants.Direction.Negative && !IsSpinning)
					{
						PushingObject = this;
					}
					break;
					
				case 3:
					Speed.Y -= wallDist;
					break;
			}
		}
		else if (GroundSpeed > 0)
		{
			var wallDist = castDir switch
			{
				0 => tile_find_h(x + Speed.X + wallRadius, y + Speed.Y + offsetY, true, TileLayer, GroundMode)[0],
				1 => tile_find_v(x + Speed.X, y + Speed.Y - wallRadius, false, TileLayer, GroundMode)[0],
				2 => tile_find_h(x + Speed.X - wallRadius, y + Speed.Y, false, TileLayer, GroundMode)[0],
				3 => tile_find_v(x + Speed.X, y + Speed.Y + wallRadius, true, TileLayer, GroundMode)[0],
				_ => 0
			};

			if (wallDist >= 0) return;
			
			switch (MathB.GetAngleQuadrant(Angle))
			{
				case 0:
					Speed.X += wallDist;
					GroundSpeed = 0;
						
					if (Facing == Constants.Direction.Positive && !IsSpinning)
					{
						PushingObject = this;
					}
					break;
					
				case 1:
					Speed.Y -= wallDist;
					break;
					
				case 2:
					Speed.X -= wallDist;
					GroundSpeed = 0;
						
					if (Facing == Constants.Direction.Positive && !IsSpinning)
					{
						PushingObject = this;
					}
					break;
						
				case 3:
					Speed.Y += wallDist;
					break;
			}
		}
	}

	private void ProcessRollStart()
	{
		if (!IsGrounded || IsSpinning || Action is Actions.SpinDash or Actions.HammerRush) return;
		
		if (!IsForcedRoll && (InputDown.Left || InputDown.Right)) return;
	
		var allowSpin = false;
		if (InputDown.Down)
		{
			if (SharedData.PlayerPhysics == PhysicsTypes.SK)
			{
				if (Math.Abs(GroundSpeed) >= 1f)
				{
					allowSpin = true;
				}
				else
				{
					Animation = Animations.Duck;
				}
			}
			else if (Math.Abs(GroundSpeed) >= 0.5f)
			{
				allowSpin = true;
			}
		}

		if (!allowSpin && !IsForcedRoll) return;
		Position += new Vector2(0f,  Radius.Y - RadiusSpin.Y);
		Radius.Y = RadiusSpin.Y;
		Radius.X = RadiusSpin.X;
		IsSpinning = true;
		Animation = Animations.Spin;
			
		//TODO: audio
		//audio_play_sfx(sfx_roll);
	}

	private void ProcessLevelBound()
	{
		if (IsDead) return;
	
		var camera = Camera.MainCamera;
	
		// Note that position here is checked including subpixel
		if (Position.X + Speed.X < camera.Limit.X + 16f)
		{
			GroundSpeed = 0;
			Speed.X = 0;
			Position = new Vector2(camera.Limit.X + 16f, Position.Y);
		}
	
		float rightBound = camera.Limit.Z - 24f;
		//TODO: replace instance_exists
		/*if (instance_exists(obj_signpost))
		{
			// TODO: There should be a better way?
			rightBound += 64;
		}*/
	
		if (Position.X + Speed.X > rightBound)
		{
			GroundSpeed = 0;
			Speed.X = 0;
			Position = new Vector2(rightBound, Position.Y);
		}
	
		if (Action is Actions.Flight or Actions.Climb)
		{
			if (Position.Y + Speed.Y < camera.Limit.Y + 16)
			{ 	
				if (Action == Actions.Flight)
				{
					Gravity	= GravityType.TailsDown;
				}
			
				Speed.Y = 0;
				Position = new Vector2(Position.X, camera.Limit.Y + 16);
			}
		}
		else if (Action == Actions.Glide && Position.Y < camera.Limit.Y + 10)
		{
			Speed.X = 0;
		}
	
		if (AirTimer > 0 && Position.Y > Math.Max(camera.Limit.W, camera.Bound.Z))
		{
			Kill();
		}
	}

	private void ProcessPosition()
	{
		if (Action == Actions.Carried) return;
	
		if (StickToConvex)
		{
			Speed = Speed.Clamp(new Vector2(-16f, -16f), new Vector2(16, 16));
		}

		Position += Speed;
	
		if (!IsGrounded && Action != Actions.Carried)
		{
			Speed.Y += Gravity;
		}
	}

	private void ProcessCollisionGroundFloor()
	{
		// Control routine checks
		if (!IsGrounded || OnObject != null) return;

		GroundMode = Angle switch
		{
			<= 45 or >= 315 => Constants.GroundMode.Floor,
			> 45 and < 135 => Constants.GroundMode.RightWall,
			>= 135 and <= 225 => Constants.GroundMode.Ceiling,
			_ => Constants.GroundMode.LeftWall
		};

		const int minTolerance = 4;
		const int maxTolerance = 14;
		
		switch (GroundMode)
		{
			case Constants.GroundMode.Floor:
				var _floor_data = tile_find_2v(x - Radius.X, y + Radius.Y, x + Radius.X, y + Radius.Y, true, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if (!StickToConvex)
				{
					float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
						maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(Speed.X)), maxTolerance);
					
					if (_floor_dist > tolerance)
					{
						PushingObject = null;
						IsGrounded = false;
						
						Sprite.UpdateFrame(0);
						break;
					}		
				}

				if (_floor_dist < -maxTolerance) break;
				
				if (SharedData.PlayerPhysics >= PhysicsTypes.S2)
				{
					_floor_angle = SnapFloorAngle(_floor_angle);
				}
				
				Position += new Vector2(0f, _floor_dist);
				Angle = _floor_angle;
				break;
			
			case Constants.GroundMode.RightWall:
				var _floor_data = tile_find_2h(x + Radius.Y, y + Radius.X, x + Radius.Y, y - Radius.X, true, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if (!StickToConvex)
				{
					float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
						maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(Speed.Y)), maxTolerance);
					if (_floor_dist > tolerance)
					{
						PushingObject = null;
						IsGrounded = false;
						
						Sprite.UpdateFrame(0);
						break;
					}	
				}
				
				if (_floor_dist < -maxTolerance) break;

				if (SharedData.PlayerPhysics >= PhysicsTypes.S2)
				{
					_floor_angle = SnapFloorAngle(_floor_angle);
				}
				
				Position += new Vector2(_floor_dist, 0f);
				Angle = _floor_angle;
				break;
			
			case Constants.GroundMode.Ceiling:
				var _floor_data = tile_find_2v(x + Radius.X, y - Radius.Y, x - Radius.X, y - Radius.Y, false, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if (!StickToConvex)
				{
					float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
						maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(Speed.X)), maxTolerance);
					if (_floor_dist > tolerance)
					{
						PushingObject = null;
						IsGrounded = false;
						
						Sprite.UpdateFrame(0);
						break;
					}
				}
				
				if (_floor_dist < -maxTolerance) break;

				if (SharedData.PlayerPhysics >= PhysicsTypes.S2)
				{
					_floor_angle = SnapFloorAngle(_floor_angle);
				}
				
				Position -= new Vector2(0, _floor_dist);
				Angle = _floor_angle;
				break;
			
			case Constants.GroundMode.LeftWall:
				var _floor_data = tile_find_2h(x - Radius.Y, y - Radius.X, x - Radius.Y, y + Radius.X, false, TileLayer, GroundMode);
				var _floor_dist = _floor_data[0];
				var _floor_angle = _floor_data[1];
				
				if (!StickToConvex)
				{
					float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
						maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(Speed.Y)), maxTolerance);
					if (_floor_dist > tolerance)
					{
						PushingObject = null;
						IsGrounded = false;
						
						Sprite.UpdateFrame(0);
						break;
					}
				}
				
			    if (_floor_dist < -maxTolerance) break;

				if (SharedData.PlayerPhysics >= PhysicsTypes.S2)
				{
					_floor_angle = SnapFloorAngle(_floor_angle);
				}

				Position -= new Vector2(_floor_dist, 0f);
				Angle = _floor_angle;
				break;
		}
	}

	private float SnapFloorAngle(float floorAngle)
	{
		float difference = Math.Abs(Angle % 180f - floorAngle % 180f);
		
		if (difference is <= 45 or >= 135) return floorAngle;
		
		floorAngle = MathF.Round(Angle / 90f) % 4f * 90f;
		if (floorAngle == 0f)
		{
			floorAngle = 360;
		}

		return floorAngle;
	}

	private void ProcessSlopeRepel()
	{
		if (!IsGrounded || StickToConvex || Action == Actions.HammerRush) return;
	
		if (GroundLockTimer > 0f)
		{
			GroundLockTimer--;
		}
		else if (Math.Abs(GroundSpeed) < 2.5f)
		{
			if (SharedData.PlayerPhysics < PhysicsTypes.S3)
			{
				if (MathB.GetAngleQuadrant(Angle) == 0) return;
				
				GroundSpeed = 0f;	
				GroundLockTimer = 30f;
				IsGrounded = false;
			}
			else if (Angle is > 33.75f and <= 326.25f)
			{
				if (Angle is > 67.5f and <= 292.5f)
				{
					IsGrounded = false;
				}
				else
				{
					GroundSpeed += Angle < 180f ? -0.5f : 0.5f;
				}
		
				GroundLockTimer = 30;
			}
		}
	}

	private void ProcessCollisionAir()
	{
		// Control routine checks
		if (IsGrounded || IsDead) return;
		
		// Action checks
		if (Action is Actions.Glide or Actions.Climb) return;
		
		var _wall_radius = RadiusNormal.X + 1;
		var _move_vector = math_get_vector_256(Speed.X, Speed.Y);
		var _move_quad = MathB.GetAngleQuadrant(_move_vector);
		
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
				if _move_quad == 2 && MathB.GetAngleQuadrant(_roof_angle) % 2 > 0 && Action != Actions.Flight
				{
					Angle = _roof_angle;
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
					
				if MathB.GetAngleQuadrant(_floor_angle) > 0
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
			Angle = _floor_angle;
			
			player_land(id);
		}
	}

	private void ProcessGlideCollision()
	{
		// This script is a modified copy of scr_player_collision_air()
		
		if (Action != Actions.Glide) return;
		
		var _wall_radius = RadiusNormal.X + 1;
		var _move_vector = math_get_vector_256(Speed.X, Speed.Y);
		var _move_quad = MathB.GetAngleQuadrant(_move_vector);
		
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
					Angle = _floor_angle;
				}
				
				return;
			}

			if _floor_dist < 0
			{
				y += _floor_dist;
				Angle = _floor_angle;
				Speed.Y = 0;
				_collision_flag_floor = true;
			}
		}
		
		// Land logic
		if _collision_flag_floor
		{
			if ActionState == GlideStates.Air
			{
				if MathB.GetAngleQuadrant(Angle) == 0
				{
					Animation = Animations.GlideGround;
					ActionState = GLIDE_STATE_GROUND;
					ActionValue = 0;
					Gravity = 0;
				}
				else
				{
					GroundSpeed = Angle < 180 ? Speed.X : -Speed.X;
					player_land(id);
				}
			}
			else if ActionState == GlideStates.Fall
			{
				player_land(id);
				audio_play_sfx(sfx_land);
				
				if MathB.GetAngleQuadrant(Angle) == 0
				{
					Animation = ANI_GLIDE_LAND;
					GroundLockTimer = 16;
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
			if ActionState != GlideStates.Air
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
			
			if Facing == Constants.Direction.Negative
			{
				x++;
			}
			
			Animation = ANI_CLIMB_WALL;
			Action = Actions.Climb;
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
		Animation = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0;
		Radius.X = RadiusNormal.X;
		Radius.Y = RadiusNormal.Y;
		
		ResetGravity();
	}

	private void ProcessCarry()
	{
		if (Type != Types.Tails || carry_timer > 0 && --carry_timer != 0) return;
	
		if CarryTarget == null
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
				if _player.Action == Actions.SpinDash || _player.Action == Actions.Carried 
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
				_player.Action = Actions.Carried ;
				CarryTarget = _player;
			
				with _player
				{
					sub_PlayerCarryAttachTo(other);
				}
			}
		}
		else
		{
			with CarryTarget
			{
				var _tails = other;
				var _previous_x = other.carry_target_x;
				var _previous_y = other.carry_target_y;	
			
				var InputPress = player_get_input(0);
				if InputPress.Abc
				{
					_tails.CarryTarget = null;
					_tails.carry_timer = 18;
				
					IsSpinning = true;
					IsJumping = true;
					Action = Actions.None;
					Animation = Animations.Spin;
					Radius.X = RadiusSpin.X;
					Radius.Y = RadiusSpin.Y;
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
					_tails.CarryTarget = null;
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
		if IsDead || !instance_exists(c_stage) || !c_stage.water_enabled
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
					if !(Action == Actions.Flight || Action == Actions.Glide && ActionState != GlideStates.Fall)
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
			if AirTimer > -1
			{
				AirTimer--;
			}
			
			switch AirTimer
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
					Camera.MainCamera.target = null;
				
				return;
				
				case -1:
				
					if floor(y) > Camera.MainCamera.view_y + SharedData.game_height + 276
					{
						if Id == 0
						{
							FrameworkData.update_effects = false;
							FrameworkData.UpdateObjects = false;
							FrameworkData.allow_pause = false;
						}
						
						IsDead = true;
					}
				
				return;
			}
		}
			
		if floor(y) < c_stage.water_level
		{
			if !is_hurt && Action != Actions.Glide
			{
				if SharedData.PlayerPhysics <= PhysicsTypes.S2 || Speed.Y >= -4
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
			AirTimer = AIR_VALUE_MAX;
			
			sub_PlayerWaterSplash();
		}
	}

	private void ProcessWaterSplash()
	{
		if (Action != Actions.Climb && Action != Actions.Glide)
		{
			//TODO: obj_water_splash
			//instance_create(x, c_stage.water_level, obj_water_splash);
		}
		
		//TODO: audio
		//audio_play_sfx(sfx_water_splash);
	}

	private void UpdateCollision()
	{
		if (IsDead) return;
	
		if (Animation != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitbox(new Vector2I(8, Radius.Y - 3));
		}
		else if (Type != Types.Tails && Type != Types.Amy)
		{
			SetHitbox(new Vector2I(8, 10), new Vector2I(0, 6));
		}
	
		// Clear extra hitbox
		if (Animation != Animations.HammerRush && Animation != Animations.HammerSpin && Barrier.State != Barrier.States.DoubleSpin)
		{
			SetHitboxExtra(new Vector2I(0, 0));
		}

		SetSolid(new Vector2I(RadiusNormal.X + 1, Radius.Y));
	}

	private void RecordData()
	{
		if (IsDead) return;
	
		//TODO: record data
		//ds_list_insert(ds_record_data, 0, [x, y, player_get_input(0), player_get_input(1), PushingObject, Facing]);
		//ds_list_delete(ds_record_data, 32);
	}

	private void ProcessRotation()
	{
		if (Animation != Animations.Move)
		{
			VisualAngle = 360f;
		}
		else
		{
			if (IsGrounded && SharedData.RotationMode > 0f)
			{
				var angle = 360f;
				float step;
			
				if (Angle is > 22.5f and <= 337.5f)
				{
					angle = Angle;
					step = 2f - Math.Abs(GroundSpeed) * 3f / 32f;
				}
				else
				{
					step = 2f - Math.Abs(GroundSpeed) / 16f;
				}

				float radians = Mathf.DegToRad(angle);
				float radiansVisual = Mathf.DegToRad(VisualAngle);
				VisualAngle = MathF.Atan2(
					Mathf.DegToRad(MathF.Sin(radians) + MathF.Sin(radiansVisual) * step), 
					Mathf.DegToRad(MathF.Cos(radians) + MathF.Cos(radiansVisual) * step));
			}
			else
			{
				VisualAngle = Angle;
			}
		}
	
		if (SharedData.RotationMode > 0)
		{
			RotationDegrees = VisualAngle;
		}
		else
		{
			RotationDegrees = MathF.Ceiling((VisualAngle - 22.5f) / 45f) * 45f;
		}
	}

	private void ProcessAnimate()
	{
		if (FrameworkData.UpdateObjects)
		{
			if (animation_buffer == -1 && AnimationTimer > 0f)
			{
				animation_buffer = Animation;
			}
		
			if (AnimationTimer < 0)
			{
				if (Animation == animation_buffer)
				{
					Animation = Animations.Move;
				}
			
				AnimationTimer = 0;
				animation_buffer = -1;
			}
			else if (animation_buffer != -1)
			{
				AnimationTimer--;
			}
		}
	
		if (Animation != Animations.Spin || Mathf.IsEqualApprox(Sprite.GetTimer(), Sprite.GetDuration()))
		{
			Sprite.Scale = new Vector2(Math.Abs(Sprite.Scale.X) * (float)Facing, Sprite.Scale.Y);
		}
	
		switch (Type)
		{
			case Types.Sonic:
				if (IsSuper)
				{
					scr_player_animate_supersonic();
				}
				else
				{
					scr_player_animate_sonic();
				}
				break;
		
			case Types.Tails:
				scr_player_animate_tails();
				break;
		
			case Types.Knuckles:
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
			newObject.Scale = new Vector2(newObject.Scale.X * (int)Facing, newObject.Scale.Y);
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
				Math.Clamp(Speed.X, -16f, 16f), 
				Math.Clamp(Speed.Y, -16f, 16f));
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
			float radians = Mathf.DegToRad(Angle);
			Speed = new Vector2(MathF.Sin(radians), MathF.Sin(radians)) * force;

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
			GroundSpeed	= 6 * (int)Facing;
		}

		if (IsSpinning) return;
		Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);

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
		else if (Mathf.IsEqualApprox(ActionValue, MaxDropDashCharge)) // Called from player_land() function
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
			if (Mathf.IsEqualApprox(ActionValue, MaxDropDashCharge))
			{
				Animation = Animations.Spin;
				Action = Actions.DropDashCancel;
			}
			
			ActionValue = 0;
		}
	}
	
	private void ReleaseDropDash()
	{
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
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
			else if (!Mathf.IsEqualApprox(Angle, 360f))
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
			else if (!Mathf.IsEqualApprox(Angle, 360f))
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
			if (!Mathf.IsEqualApprox(ActionValue, maxHammerSpinCharge)) return;
			
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
