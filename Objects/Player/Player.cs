using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.CommonObject;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using OrbinautFramework3.Objects.Spawnable.PlayerParticles;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : CommonObject
{
	#region Constants

	private const byte EditModeAccelerationMultiplier = 4;
	private const float EditModeAcceleration = 0.046875f;
	private const byte EditModeSpeedLimit = 16;
	private const byte MaxDropDashCharge = 20;
	private const byte DefaultViewTime = 120;
	
	public const byte CpuDelay = 16;
	
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
		None,
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
	[Export] public AnimatedSprite Sprite { get; private set; }

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

	public Constants.TileLayers TileLayer { get; set; }
	public Constants.GroundMode GroundMode { get; set; }
	public bool StickToConvex { get; set; }
    
	public bool ObjectInteraction { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public CommonObject PushingObject { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public CommonObject OnObject { get; set; }
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
	public Animations AnimationBuffer { get; set; }
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
	public Vector2 CarryTargetPosition { get; set; }
    
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
	
	public CollisionTileMap TileMap { get; set; }
	public CommonStage Stage { get; set; }
	public TileCollider TileCollider { get; set; }

	public Dictionary<CommonObject, Constants.TouchState> TouchObjects { get; private set; }

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

		if (GetOwner<Node>() is CommonScene scene)
		{
			TileMap = scene.CollisionTileMap;
			if (scene is CommonStage stage)
			{
				Stage = stage;
			}
		}

		TileCollider = new TileCollider();
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		
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
		TileLayer = Constants.TileLayers.Main;
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
		
		LifeRewards = [RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000];

		TouchObjects = [];
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Id = Players.Count;
		Players.Add(this);
		FrameworkData.CurrentScene.AddPlayerStep(this);
		FrameworkData.CurrentScene.PreUpdate += _ =>
		{
			TouchObjects.Clear();
		};
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

	public void PlayerStep(float processSpeed)
	{
		if (FrameworkData.IsPaused || !FrameworkData.UpdateObjects && !IsDead) return;
		
		// Process local input
		UpdateInput();

		// Process Edit Mode
		if (ProcessEditMode(processSpeed)) return;
	    
		// Process CPU Player logic (return if flying in or respawning)
		if (ProcessAI(processSpeed)) return;
	    
		// Process Restart Event
		ProcessRestart(processSpeed);
	    
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
		if (Action is Actions.SpinDash or Actions.PeelOut || IsForcedRoll || !IsGrounded) return false;
		
		if (!InputPress.Abc || !CheckCeilingDistance()) return false;
		
		if (!SharedData.FixJumpSize)
		{
			// Why they even do that???
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
		Speed += PhysicParams.JumpVelocity * new Vector2(MathF.Sin(radians), MathF.Cos(radians));
		
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

	private bool CheckCeilingDistance()
	{
		if (GroundMode == Constants.GroundMode.Ceiling) return true;
		
		return CalculateCellDistance() >= 6; // Target ceiling distance
	}

	private int CalculateCellDistance()
	{
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap, GroundMode);
		
		return GroundMode switch
		{
			Constants.GroundMode.Floor => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1),
				Radius.Shuffle( 1, -1),
				true, Constants.Direction.Negative),
			
			Constants.GroundMode.RightWall => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1, true),
				Radius.Shuffle(1, -1, true),
				false, Constants.Direction.Negative),
			
			Constants.GroundMode.LeftWall => TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, 1, true),
				Radius.Shuffle(1, 1, true),
				false, Constants.Direction.Positive),
			
			Constants.GroundMode.Ceiling => throw new ArgumentOutOfRangeException(),
			
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void ProcessDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return;
	
		const float maxDropDashCharge = 20f;
	
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
			else if (ActionValue > 0f)
			{
				if (Mathf.IsEqualApprox(ActionValue, maxDropDashCharge))
				{		
					Animation = Animations.Spin;
					Action = Actions.DropDashCancel;
				}
			
				ActionValue = 0f;
			}
		}
	
		// Called from player_land() function
		else if (Mathf.IsEqualApprox(ActionValue, maxDropDashCharge))
		{
			Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
			Radius = RadiusSpin;

			if (IsSuper)
			{
				UpdateDropDashGroundSpeed(13f, 12f);
				Camera.MainCamera.ShakeTimer = 6;
			}
			else
			{
				UpdateDropDashGroundSpeed(12f, 8f);
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

	private void UpdateDropDashGroundSpeed(float limitSpeed, float force)
	{
		var sign = (float)Facing;
		limitSpeed *= sign;
		force *= sign;
		
		if (Speed.X * sign >= 0f)
		{
			GroundSpeed = Mathf.Floor(GroundSpeed / 4f) + force;
			if (GroundSpeed * sign <= limitSpeed) return;
			GroundSpeed = limitSpeed;
			return;
		}

		GroundSpeed = force;
		if (Mathf.IsEqualApprox(Angle, 360f)) return;
		
		GroundSpeed += Mathf.Floor(GroundSpeed / 2f);
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
					else if (Speed.Y < -1f)
					{
						Gravity = GravityType.TailsDown;
					}
				
					Speed.Y = Math.Max(Speed.Y, -4f);
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
				ClimbNormal();
				break;
			
			case ClimbStates.Ledge:
				switch (ActionValue++)
				{
					case 0f: // Frame 0
						Animation = Animations.ClimbLedge;
						Position += new Vector2(3f * (float)Facing, -2f);
						break;
					
					case 6f: // Frame 1
						Position += new Vector2(8f * (float)Facing, -10f);
						break;
					
					case 12f: // Frame 2
						Position -= new Vector2(8f * (float)Facing, 12f);
						break;
					
					case 18f: // End
						Land();
						Animation = Animations.Idle;
						Position += new Vector2(8f * (float)Facing, 4f);
						break;
				}
				break;
		}
	}

	private void ClimbNormal()
	{
		if (!Mathf.IsEqualApprox(Position.X, PreviousPosition.X))
		{
			ReleaseClimb();
			return;
		}
		
		//TODO: check GetFrameCount
		const int stepsPerFrame = 4;
		UpdateSpeedYOnClimb(Sprite.SpriteFrames.GetFrameCount(Sprite.Animation) * stepsPerFrame);
		
		int radiusX = Radius.X;
		if (Facing == Constants.Direction.Negative)
		{
			radiusX++;
		}
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);

		if (Speed.Y < 0 ? ClimbUpOntoWall(radiusX) : ReleaseClimbing(radiusX)) return;
		
		if (!InputPress.Abc)
		{
			if (Speed.Y != 0)
			{
				Sprite.UpdateFrame(Mathf.FloorToInt(ActionValue / stepsPerFrame));
			}
			return;
		}
		
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

	private bool ClimbUpOntoWall(int radiusX)
	{
		// If the wall is far away from Knuckles then he must have reached a ledge, make him climb up onto it
		var offset = new Vector2I(radiusX * (int)Facing, -Radius.Y - 1);
		int wallDistance = TileCollider.FindDistance(offset, false, Facing);
			
		if (wallDistance >= 4)
		{
			ActionState = (int)ClimbStates.Ledge;
			ActionValue = 0;
			Speed.Y = 0;
			return true;
		}

		// If Knuckles has encountered a small dip in the wall, cancel climb movement
		if (wallDistance != 0)
		{
			Speed.Y = 0;
		}

		// If Knuckles has bumped into the ceiling, cancel climb movement and push him out
		offset = new Vector2I(radiusX * (int)Facing, 1 - RadiusNormal.Y);
		int ceilDistance = TileCollider.FindDistance(offset, true, Constants.Direction.Negative);

		if (ceilDistance >= 0) return false;
		Position -= new Vector2(0f, ceilDistance);
		Speed.Y = 0;
		return false;
	}

	private bool ReleaseClimbing(int radiusX)
	{
		// If Knuckles is no longer against the wall, make him let go
		var offset = new Vector2I(radiusX * (int)Facing, Radius.Y + 1);
		int wallDistance = TileCollider.FindDistance(offset, false, Facing);
			
		if (wallDistance != 0)
		{
			ReleaseClimb();
			return true;
		}
			
		// If Knuckles has reached the floor, make him land
		offset = new Vector2I(radiusX * (int)Facing, RadiusNormal.Y);
		(int distance, float angle) = TileCollider.FindTile(offset, true, Constants.Direction.Positive);

		if (distance >= 0) return false;
		Position += new Vector2(0f, distance + RadiusNormal.Y - Radius.Y);
		Angle = angle;
				
		Land();

		Animation = Animations.Idle;
		Speed.Y = 0;
				
		return true;
	}

	private void UpdateSpeedYOnClimb(int maxValue)
	{
		if (InputDown.Up)
		{
			if (++ActionValue > maxValue)
			{
				ActionValue = 0f;
			}
					
			Speed.Y = -PhysicParams.AccelerationClimb;
			return;
		}
		if (InputDown.Down)
		{
			if (--ActionValue < 0f)
			{
				ActionValue = maxValue;
			}
					
			Speed.Y = PhysicParams.AccelerationClimb;
			return;
		}

		Speed.Y = 0;
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
		
		switch ((GlideStates)ActionState)
		{
			case GlideStates.Air: GlideAir(); break;
			case GlideStates.Ground: GlideGround(); break;
			default: throw new ArgumentOutOfRangeException();
		}
	}

	private void GlideAir()
	{
		const float glideGravity = 0.125f;
		
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

		GlideAirTurnAround();
				
		// Update horizontal and vertical speed
		Speed.X = GroundSpeed * -Mathf.Cos(Mathf.DegToRad(ActionValue));
		Gravity = Speed.Y < 0.5f ? glideGravity : -glideGravity;
				
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

		if (InputDown.Abc) return;
		
		Animation = Animations.GlideFall;
		ActionState = (int)GlideStates.Fall;
		ActionValue = 0f;
		Radius = RadiusNormal;
		Speed.X *= 0.25f;

		ResetGravity();
	}

	private void GlideGround()
	{
		GlideGroundUpdateSpeedX();
				
		if (Speed.X == 0f)
		{
			Land();
			Sprite.UpdateFrame(1);

			Animation = Animations.GlideGround;
			GroundLockTimer = 16;
			GroundSpeed = 0;

			return;
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
	}

	private void GlideGroundUpdateSpeedX()
	{
		const float slideFriction = 0.09375f;
		
		if (!InputDown.Abc)
		{
			Speed.X = 0f;
			return;
		}

		Speed.X = Speed.X switch
		{
			> 0f => Math.Max(0f, Speed.X - slideFriction),
			< 0f => Math.Min(0f, Speed.X + slideFriction),
			_ => Speed.X
		};
	}

	private void GlideAirTurnAround()
	{
		const float angleIncrement = 2.8125f;
		
		if (InputDown.Left && !Mathf.IsZeroApprox(ActionValue))
		{
			ActionValue = (ActionValue > 0f ? -ActionValue : ActionValue) + angleIncrement;
			return;
		}
		
		if (InputDown.Right && !Mathf.IsEqualApprox(ActionValue, 180f))
		{
			ActionValue = (ActionValue < 0f ? -ActionValue : ActionValue) + angleIncrement;
			return;
		}
		
		if (Mathf.IsZeroApprox(ActionValue % 180f)) return;
		ActionValue += angleIncrement;
	}
	
	private void ProcessHammerSpin()
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

		if (!IsGrounded)
		{
			if (Speed.X != 0f)
			{
				Speed.X = 6f * Math.Sign(GroundSpeed);
				return;
			}
		
			CancelHammerRush();
			return;
		}

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

	private void CancelHammerRush()
	{
		Animation = Animations.Move;
		Action = Actions.None;
	}

	private void ProcessSlopeResist()
	{
		if (!IsGrounded || IsSpinning || Angle is > 135f and <= 225f) return;
	
		if (Action is Actions.HammerRush or Actions.PeelOut) return;
		
		float slopeGrv = 0.125f * MathF.Sin(Mathf.DegToRad(Angle));
		if (GroundSpeed != 0f || SharedData.PlayerPhysics >= PhysicsTypes.S3 && Math.Abs(slopeGrv) > 0.05078125f)
		{
			GroundSpeed -= slopeGrv;
		}
	}

	private void ProcessSlopeResistRoll()
	{
		if (!IsGrounded || !IsSpinning || Angle is > 135f and <= 225f) return;
	
		float angleSine = MathF.Sin(Mathf.DegToRad(Angle));
		float slopeGrv = Math.Sign(GroundSpeed) != Math.Sign(angleSine) ? 0.3125f : 0.078125f;
		GroundSpeed -= slopeGrv * angleSine;
	}

	private void ProcessMovementGround()
	{
		// Control routine checks
		if (!IsGrounded || IsSpinning) return;
		
		// Action checks
		if (Action is Actions.SpinDash or Actions.PeelOut or Actions.HammerRush) return;
		
		// If Knuckles is standing up from a slide and DOWN button is pressed, cancel
		// control lock. This allows him to Spin Dash
		if (Animation == Animations.GlideGround && InputDown.Down)
		{
			GroundLockTimer = 0f;
		}
		
		if (Mathf.IsZeroApprox(GroundLockTimer))
		{
			var doSkid = false;
			
			// Move left
			if (InputDown.Left)
			{	
				doSkid = MoveOnGround(Constants.Direction.Negative);
			}
			
			// Move right
			if (InputDown.Right)
			{
				doSkid = MoveOnGround(Constants.Direction.Positive);
			}
			
			UpdateMovementGroundAnimation(doSkid);
			SetPushAnimation();
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

	private bool MoveOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		
		if (GroundSpeed * sign < 0f)
		{
			GroundSpeed += PhysicParams.Deceleration * sign;
			if (GroundSpeed * sign >= 0f)
			{
				GroundSpeed = 0.5f * sign;
			}
					
			return true;
		}

		if (!SharedData.NoSpeedCap || GroundSpeed * sign < PhysicParams.AccelerationTop)
		{
			GroundSpeed = direction == Constants.Direction.Positive
				? Math.Min(GroundSpeed + PhysicParams.Acceleration,  PhysicParams.AccelerationTop)
				: Math.Max(GroundSpeed - PhysicParams.Acceleration, -PhysicParams.AccelerationTop);
		}

		if (Facing == direction) return false;
		
		Animation = Animations.Move;
		Facing = direction;
		PushingObject = null;
					
		Sprite.UpdateFrame(0);

		return false;
	}

	private void UpdateMovementGroundAnimation(bool doSkid)
	{
		if (Angles.GetQuadrant(Angle) != 0f)
		{
			if (Animation is Animations.Skid or Animations.Push) return;
			Animation = Animations.Move;
			return;
		}
		
		if (doSkid && Math.Abs(GroundSpeed) >= 4f && Animation != Animations.Skid)
		{
			AnimationTimer = Type == Types.Sonic ? 24f : 16f;
			Animation = Animations.Skid;
					
			//TODO: audio
			//audio_play_sfx(sfx_skid);
		}
				
		if (GroundSpeed != 0f)
		{
			// TODO: This
			if (Animation is Animations.Skid or Animations.Push) return;
			Animation = Animations.Move;
			return;
		}
		
		PushingObject = null;
		Animation = InputDown.Up ? Animations.LookUp : InputDown.Down ? Animations.Duck : Animations.Idle;
	}

	private void SetPushAnimation()
	{
		if (PushingObject == null)
		{
			if (Animation != Animations.Push) return;
			Animation = Animations.Move;
			return;
		}
		
		if (Animation != Animations.Move || !Mathf.IsEqualApprox(Sprite.GetTimer(), Sprite.GetDuration())) return;
		Animation = Animations.Push;
	}

	private void ProcessMovementGroundRoll()
	{
		// Control routine checks
		if (!IsGrounded || !IsSpinning) return;

		if (GroundLockTimer == 0f)
		{
			if (InputDown.Left)
			{
				RollOnGround(Constants.Direction.Negative); // Move left
			}
			
			if (InputDown.Right)
			{
				RollOnGround(Constants.Direction.Positive); // Move right
			}
		}
	
		// Apply friction
		GroundSpeed = GroundSpeed switch
		{
			> 0 => Math.Max(GroundSpeed - PhysicParams.FrictionRoll, 0),
			< 0 => Math.Min(GroundSpeed + PhysicParams.FrictionRoll, 0),
			_ => GroundSpeed
		};

		UpdateSpinningOnGround();
	
		float radians = Mathf.DegToRad(Angle);
		Speed = GroundSpeed * new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
		Speed.X = Math.Clamp(Speed.X, -16f, 16f);
	}

	private void RollOnGround(Constants.Direction direction)
	{
		var sign = (float)direction;
		if (sign * GroundSpeed >= 0f)
		{
			Facing = direction;
			PushingObject = null;
			return;
		}

		GroundSpeed += sign * PhysicParams.DecelerationRoll;
		if (sign * GroundSpeed < 0f) return;
		GroundSpeed = sign * 0.5f;
	}

	private void UpdateSpinningOnGround()
	{
		// Stop spinning
		if (!IsForcedRoll)
		{
			if (GroundSpeed != 0f)
			{
				if (SharedData.PlayerPhysics != PhysicsTypes.SK || Math.Abs(GroundSpeed) >= 0.5f) return;
			}
			
			Position += new Vector2(0f, Radius.Y - RadiusNormal.Y);

			Radius = RadiusNormal;
			
			IsSpinning = false;
			Animation = Animations.Idle;
			return;
		}
	
		// If forced to spin, keep moving player
		if (SharedData.PlayerPhysics == PhysicsTypes.CD)
		{
			if (GroundSpeed is >= 0f and < 2f)
			{
				GroundSpeed = 2f;
			}
			return;
		}
		
		if (GroundSpeed != 0f) return;
		GroundSpeed = SharedData.PlayerPhysics == PhysicsTypes.S1 ? 2f : 4f * (float)Facing;
	}

	private void ProcessMovementAir()
	{
		// Control routine checks
		if (IsGrounded || IsDead) return;
	
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
	
		if (OnObject != null)
		{
			// TODO: check IsInstanceValid == instance_exist
			if (!IsInstanceValid(OnObject) || OnObject.SolidData.NoBalance) return;
	
			const int leftEdge = 2;
			int rightEdge = OnObject.SolidData.Radius.X * 2 - leftEdge;
			int playerX = Mathf.FloorToInt(OnObject.SolidData.Radius.X - OnObject.Position.X + Position.X);
		
			if (playerX < leftEdge)
			{
				BalanceToDirection(Constants.Direction.Negative, playerX < leftEdge - 4);
			}
			else if (playerX > rightEdge)
			{
				BalanceToDirection(Constants.Direction.Positive, playerX > rightEdge + 4);
			}
		}
		
		if (Angles.GetQuadrant(Angle) > 0) return;
			
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer, TileMap);
		int floorDist = TileCollider.FindDistance(new Vector2I(), true, Constants.Direction.Positive);	
		if (floorDist < 12) return;

		const Constants.Direction direction = Constants.Direction.Positive;
			
		int distanceLeft = TileCollider.FindDistance(new Vector2I(-Radius.X, 0), true, direction);
		int distanceRight = TileCollider.FindDistance(new Vector2I(Radius.X, 0), true, direction);
		
		if (distanceLeft != Constants.TileSize * 2 && distanceRight != Constants.TileSize * 2) return;

		int offsetX = distanceLeft == Constants.TileSize * 2 ? 6 : -6;
		bool isPanic = TileCollider.FindDistance(new Vector2I(offsetX, 0), true, direction) >= 12;
		BalanceToDirection(Constants.Direction.Negative, isPanic);
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		if (Type != Types.Sonic || IsSuper)
		{
			Animation = Animations.Balance;
			Facing = direction;
			return;
		}
		
		if (!isPanic)
		{
			Animation = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
		}
		else if (Facing != direction)
		{
			Animation = Animations.BalanceTurn;
			Facing = direction;
		}
		else if (Animation != Animations.BalanceTurn)
		{
			Animation = Animations.BalancePanic;
		}
	}

	private void ProcessCollisionGroundWalls()
	{
		// Control routine checks
		if (!IsGrounded) return;
		
		if (SharedData.PlayerPhysics < PhysicsTypes.SK)
		{
			if (Angle is > 90f and <= 270f) return;
		}
		else if (Angle is >= 90f and <= 270f && Angle % 90f != 0f)
		{
			return;
		}

		int castDirection = Angle switch
		{
			>= 45f and <= 128f => 1,
			> 128f and < 225f => 2,
			>= 225f and < 315f => 3,
			_ => 0
		};

		int wallRadius = RadiusNormal.X + 1;
		int offsetY = 8 * (Mathf.IsEqualApprox(Angle, 360f) ? 1 : 0);

		int sign;
		Constants.Direction firstDirection, secondDirection;
		switch (GroundSpeed)
		{
			case < 0f:
				sign = (int)Constants.Direction.Positive;
				firstDirection = Constants.Direction.Negative;
				secondDirection = Constants.Direction.Positive;
				break;
			
			case > 0f:
				sign = (int)Constants.Direction.Negative;
				firstDirection = Constants.Direction.Positive;
				secondDirection = Constants.Direction.Negative;
				wallRadius *= sign;
				break;
			
			default:
				return;
		}
		
		TileCollider.SetData((Vector2I)(Position + Speed), TileLayer, TileMap, GroundMode);
		
		int wallDist = castDirection switch
		{
			0 => TileCollider.FindDistance(new Vector2I(-wallRadius, offsetY), false, firstDirection),
			1 => TileCollider.FindDistance(new Vector2I(0, wallRadius), true, secondDirection),
			2 => TileCollider.FindDistance(new Vector2I(wallRadius, 0), false, secondDirection),
			3 => TileCollider.FindDistance(new Vector2I(0, -wallRadius), true, firstDirection),
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (wallDist >= 0) return;
		
		wallDist *= sign;
		
		switch (Angles.GetQuadrant(Angle))
		{
			case 0:
				Speed.X -= wallDist;
				GroundSpeed = 0f;
					
				if (Facing == firstDirection && !IsSpinning)
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
					
				if (Facing == firstDirection && !IsSpinning)
				{
					PushingObject = this;
				}
				break;
				
			case 3:
				Speed.Y -= wallDist;
				break;
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
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap, GroundMode);

		(int distance, float angle) = GroundMode switch
		{
			Constants.GroundMode.Floor => TileCollider.FindClosestTile(new Vector2I(-Radius.X, Radius.Y),
				new Vector2I(Radius.X, Radius.Y), true, Constants.Direction.Positive),
			
			Constants.GroundMode.RightWall => TileCollider.FindClosestTile(new Vector2I(Radius.Y, Radius.X),
				new Vector2I(Radius.Y, -Radius.X), false, Constants.Direction.Positive),
			
			Constants.GroundMode.Ceiling => TileCollider.FindClosestTile(new Vector2I(Radius.X, -Radius.Y),
				new Vector2I(-Radius.X, -Radius.Y), true, Constants.Direction.Negative),
			
			Constants.GroundMode.LeftWall => TileCollider.FindClosestTile(new Vector2I(-Radius.Y, -Radius.X), 
				new Vector2I(-Radius.Y, Radius.X), false, Constants.Direction.Negative),
			
			_ => throw new ArgumentOutOfRangeException()
		};
		
		if (!StickToConvex)
		{
			float toleranceCheckSpeed = GroundMode switch
			{
				Constants.GroundMode.Floor => Speed.X,
				Constants.GroundMode.RightWall => Speed.Y,
				Constants.GroundMode.Ceiling => Speed.X,
				Constants.GroundMode.LeftWall => Speed.Y,
				_ => throw new ArgumentOutOfRangeException()
			};
			
			float tolerance = SharedData.PlayerPhysics < PhysicsTypes.S2 ? 
				maxTolerance : Math.Min(minTolerance + Math.Abs(MathF.Floor(toleranceCheckSpeed)), maxTolerance);
			
			if (distance > tolerance)
			{
				PushingObject = null;
				IsGrounded = false;
						
				Sprite.UpdateFrame(0);
				return;
			}
		}

		if (distance < -maxTolerance) return;
		
		Position += GroundMode switch
		{
			Constants.GroundMode.Floor => new Vector2(0f, distance),
			Constants.GroundMode.RightWall => new Vector2(distance, 0f),
			Constants.GroundMode.Ceiling => new Vector2(0f, -distance),
			Constants.GroundMode.LeftWall => new Vector2(-distance, 0f),
			_ => throw new ArgumentOutOfRangeException()
		};

		Angle = SharedData.PlayerPhysics >= PhysicsTypes.S2 ? SnapFloorAngle(angle) : angle;
	}

	private float SnapFloorAngle(float floorAngle)
	{
		float difference = Math.Abs(Angle % 180f - floorAngle % 180f);
		
		if (difference is <= 45f or >= 135f) return floorAngle;
		
		floorAngle = MathF.Round(Angle / 90f) % 4f * 90f;
		if (floorAngle == 0f)
		{
			floorAngle = 360f;
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
				if (Angles.GetQuadrant(Angle) == 0) return;
				
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
		
		int wallRadius = RadiusNormal.X + 1;
		byte moveQuadrant = Angles.GetQuadrant(Angles.GetVector256(Speed));
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		
		// Perform left wall collision if not moving mostly right
		if (moveQuadrant != 1)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(-wallRadius, 0), false, Constants.Direction.Negative);
			
			if (wallDistance < 0f)
			{
				Position -= new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				
				if (moveQuadrant == 3)
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if (moveQuadrant != 3)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
			
			if (wallDistance < 0f)
			{
				Position += new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0f;
				
				if (moveQuadrant == 1)
				{
					GroundSpeed = Speed.Y;
					return;
				}
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if (moveQuadrant != 0)
		{
			(int roofDistance, float roofAngle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, -1), 
				Radius.Shuffle(1, -1),
				true, Constants.Direction.Negative);
			
			if (moveQuadrant == 3 && SharedData.PlayerPhysics >= PhysicsTypes.S3 && roofDistance <= -14f)
			{
				// Perform right wall collision if moving mostly left and too far into the ceiling
				int wallDist = TileCollider.FindDistance(
					new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
				
				if (wallDist < 0)
				{
					Position += new Vector2(wallDist, 0f);
					Speed.X = 0;
					
					return;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				if (moveQuadrant == 2 && Angles.GetQuadrant(roofAngle) % 2 > 0 && Action != Actions.Flight)
				{
					Angle = roofAngle;
					GroundSpeed = roofAngle < 180 ? -Speed.Y : Speed.Y;
					Speed.Y = 0;
					
					Land();
				}
				else
				{
					if (Speed.Y < 0f)
					{
						Speed.Y = 0f;
					}
						
					if (Action == Actions.Flight)
					{
						Gravity	= GravityType.TailsDown;
					}
				}
				
				return;
			}
		}
		
		// Perform floor collision if not moving mostly up
		int distance;
		float angle;

		if (moveQuadrant == 0)
		{
			(int distanceL, float angleL) = TileCollider.FindTile(
				Radius.Shuffle(-1, 1), 
				true, Constants.Direction.Positive);
			
			(int distanceR, float angleR) = TileCollider.FindTile(
				Radius.Shuffle(1, 1), 
				true, Constants.Direction.Positive);

			if (distanceL > distanceR)
			{
				distance = distanceR;
				angle = angleR;
			}
			else
			{
				distance = distanceL;
				angle = angleL;
			}
					
			float minClip = -(Speed.Y + 8f);		
			if (distance >= 0 || minClip >= distanceL && minClip >= distanceR) return;
					
			if (Angles.GetQuadrant(angle) > 0)
			{
				if (Speed.Y > 15.75f)
				{
					Speed.Y = 15.75f;
				}
						
				GroundSpeed = angle < 180f ? -Speed.Y : Speed.Y;
				Speed.X = 0f;
			}
			else if (angle is > 22.5f and <= 337.5f)
			{
				GroundSpeed = angle < 180f ? -Speed.Y : Speed.Y;
				GroundSpeed /= 2f;
			}
			else 
			{
				GroundSpeed = Speed.X;
				Speed.Y = 0f;
			}
		}
		else if (Speed.Y >= 0)
		{
			(distance, angle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, 1), 
				Radius.Shuffle(1, 1),
				true, Constants.Direction.Positive);
			
			if (distance >= 0) return;
				
			GroundSpeed = Speed.X;
			Speed.Y = 0;
		}
		else
		{
			return;
		}

		Position += new Vector2(0f, distance);
		Angle = angle;
			
		Land();
	}

	private void ProcessGlideCollision()
	{
		// This script is a modified copy of scr_player_collision_air()
		
		if (Action != Actions.Glide) return;
		
		int wallRadius = RadiusNormal.X + 1;
		byte moveQuad = Angles.GetQuadrant(Angles.GetVector256(Speed));
		
		var collisionFlagWall = false;
		var collisionFlagFloor = false;
		var climbY = (int)Position.Y;
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		
		// Perform left wall collision if not moving mostly right
		if (moveQuad != 1)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(-wallRadius, 0), false, Constants.Direction.Negative);

			if (wallDistance < 0)
			{
				Position -= new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				collisionFlagWall = true;
			}
		}
		
		// Perform right wall collision if not moving mostly left
		if (moveQuad != 3)
		{
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
			
			if (wallDistance < 0)
			{
				Position += new Vector2(wallDistance, 0f);
				TileCollider.Position = (Vector2I)Position;
				Speed.X = 0;
				collisionFlagWall = true;
			}
		}
		
		// Perform ceiling collision if not moving mostly down
		if (moveQuad != 0)
		{
			int roofDistance = TileCollider.FindClosestDistance(
				Radius.Shuffle(-1, -1),
				Radius.Shuffle(1, -1),
				true, Constants.Direction.Negative);

			if (moveQuad == 3 && roofDistance <= -14 && SharedData.PlayerPhysics >= PhysicsTypes.S3)
			{
				// Perform right wall collision instead if moving mostly left and too far into the ceiling
				int findDistance = TileCollider.FindDistance(
					new Vector2I(wallRadius, 0), false, Constants.Direction.Positive);
				
				if (findDistance < 0)
				{
					Position += new Vector2(findDistance, 0f);
					TileCollider.Position = (Vector2I)Position;
					Speed.X = 0;
					collisionFlagWall = true;
				}
			}
			else if (roofDistance < 0)
			{
				Position -= new Vector2(0f, roofDistance);
				TileCollider.Position = (Vector2I)Position;
				if (Speed.Y < 0 || moveQuad == 2)
				{
					Speed.Y = 0;
				}
			}
		}
		
		// Perform floor collision if not moving mostly up
		if (moveQuad != 2)
		{
			(int floorDistance, float floorAngle) = TileCollider.FindClosestTile(
				Radius.Shuffle(-1, 1), 
				Radius.Shuffle(1, 1),
				true, Constants.Direction.Positive);
		
			if ((GlideStates)ActionState == GlideStates.Ground)
			{
				if (floorDistance > 14f)
				{
					ReleaseGlide();
				}
				else
				{
					Position += new Vector2(0f, floorDistance);
					Angle = floorAngle;
				}
				
				return;
			}

			if (floorDistance < 0f)
			{
				Position += new Vector2(0f, floorDistance);
				TileCollider.Position = (Vector2I)Position;
				Angle = floorAngle;
				Speed.Y = 0;
				collisionFlagFloor = true;
			}
		}
		
		// Land logic
		if (collisionFlagFloor)
		{
			switch ((GlideStates)ActionState)
			{
				case GlideStates.Air when Angles.GetQuadrant(Angle) == 0:
					Animation = Animations.GlideGround;
					ActionState = (int)GlideStates.Ground;
					ActionValue = 0;
					Gravity = 0;
					break;
				
				case GlideStates.Air:
					GroundSpeed = Angle < 180 ? Speed.X : -Speed.X;
					Land();
					break;
				
				case GlideStates.Fall:
					Land();
					//TODO: audio
					//audio_play_sfx(sfx_land);
				
					if (Angles.GetQuadrant(Angle) != 0)
					{
						GroundSpeed = Speed.X;
						break;
					}
					
					Animation = Animations.GlideLand;
					GroundLockTimer = 16;
					GroundSpeed = 0;
					Speed.X = 0;
					break;
				
				case GlideStates.Ground:
					break;
			}
		}
		
		// Wall attach logic
		else if (collisionFlagWall)
		{
			if ((GlideStates)ActionState != GlideStates.Air) return;

			// Cast a horizontal sensor just above Knuckles. If the distance returned is not 0, he is either inside the ceiling or above the floor edge
			TileCollider.Position.Y = climbY - Radius.Y;
			int wallDistance = TileCollider.FindDistance(
				new Vector2I(wallRadius * (int)Facing, 0), false, Facing);
			
			if (wallDistance != 0)
			{
				// Cast a vertical sensor now. If the distance returned is negative, Knuckles is inside
				// the ceiling, else he is above the edge
				
				// Note: _find_mode is set to 2. LBR tiles are not ignored in this case
				TileCollider.GroundMode = Constants.GroundMode.Ceiling;
				int floorDistance = TileCollider.FindDistance(
					new Vector2I((wallRadius + 1) * (int)Facing, -1), 
					true, Constants.Direction.Positive);
				
				if (floorDistance is < 0 or >= 12)
				{
					ReleaseGlide();
					return;
				}
				
				// Adjust Knuckles' Y position to place him just below the edge
				Position += new Vector2(0f, floorDistance);
			}
			
			if (Facing == Constants.Direction.Negative)
			{
				Position += new Vector2(1f, 0f);
			}
			
			Animation = Animations.ClimbWall;
			Action = Actions.Climb;
			ActionState = (int)ClimbStates.Normal;
			ActionValue = 0;
			GroundSpeed = 0;
			Speed.Y = 0;
			Gravity	= 0;
			
			//TODO: audio
			//audio_play_sfx(sfx_grab);
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
		if (Type != Types.Tails || CarryTimer > 0 && --CarryTimer != 0) return;
	
		if (CarryTarget == null)
		{
			if (Action != Actions.Flight) return;
		
			// Try to grab another player
			foreach (Player player in Players)
			{
				if (player == this) continue;

				if (player.Action is Actions.SpinDash or Actions.Carried) continue;
			
				float distanceX = Mathf.Floor(player.Position.X - Position.X);
				if (distanceX is < -16f or >= 16f) continue;
			
				float distanceY = Mathf.Floor(player.Position.Y - Position.Y);
				if (distanceY is < 32f or >= 48f) continue;
				
				player.ResetState();
				//TODO: audio
				//audio_play_sfx(sfx_grab);
			
				player.Animation = Animations.Grab;
				player.Action = Actions.Carried;
				CarryTarget = player;

				player.AttachToPlayer(this);
			}
		}
		else
		{
			CarryTarget.OnPlayerAttached(this);
		}
	}

	private void OnPlayerAttached(Player carrier)
	{
		Vector2 previousPosition = carrier.CarryTargetPosition;
				
		if (InputPress.Abc)
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 18f;
				
			IsSpinning = true;
			IsJumping = true;
			Action = Actions.None;
			Animation = Animations.Spin;
			Radius.X = RadiusSpin.X;
			Radius.Y = RadiusSpin.Y;
			Speed.X = 0f;
			Speed.Y = PhysicParams.MinimalJumpVelocity;
					
			if (InputDown.Left)
			{
				Speed.X = -2;
			}
			else if (InputDown.Right)
			{
				Speed.X = 2;
			}
					
			//TODO: audio
			//audio_play_sfx(sfx_jump);
			
		}
		else if (carrier.Action != Actions.Flight 
		    || !Mathf.IsEqualApprox(Position.X, previousPosition.X) 
		    || !Mathf.IsEqualApprox(Position.Y, previousPosition.Y))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60;
			Action = Actions.None;
		}
		else
		{
			AttachToPlayer(carrier);
		}
	}

	private void AttachToPlayer(Player carrier)
	{
		Facing = carrier.Facing;
		Speed.X = carrier.Speed.X;
		Speed.Y = carrier.Speed.Y;
		Position = carrier.Position + new Vector2(0f, 28f);
		Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
		
		carrier.CarryTargetPosition = Position;
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
			Visible = ((int)InvincibilityFrames-- & 4) >= 1 || InvincibilityFrames == 0;
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
	
		IsInvincible = InvincibilityFrames != 0 || ItemInvincibilityTimer != 0 
			|| IsHurt || IsSuper || Barrier.State == Barrier.States.DoubleSpin;
				 
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
		if (IsDead || Stage is not { IsWaterEnabled: true }) return;
		
		// On surface
		if (!IsUnderwater)
		{
			if (Mathf.Floor(Position.Y) < Stage.WaterLevel) return;
			
			IsUnderwater = true;
			
			//TODO: obj_bubbles_player
			//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
			ProcessWaterSplash();
			
			if (!IsHurt)
			{
				if (Action != Actions.Flight && !(Action == Actions.Glide && (GlideStates)ActionState != GlideStates.Fall))
				{
					Gravity = GravityType.Underwater;
				}
				
				Speed *= new Vector2(0.5f, 0.25f);
			}
			
			if (Barrier.Type is Barrier.Types.Flame or Barrier.Types.Thunder)
			{	
				if (Barrier.Type == Barrier.Types.Thunder)
				{
					//TODO: obj_water_flash
					//instance_create(x, y, obj_water_flash);
				}
				
				Barrier.Type = Barrier.Types.None;			
			}
			
			if (Action == Actions.Flight)
			{
				//TODO: audio
				//audio_stop_sfx(sfx_flight);
				//audio_stop_sfx(sfx_flight2);
			}
		}
		
		// Underwater
		if (Barrier.Type != Barrier.Types.Water)
		{
			if (AirTimer > -1f)
			{
				AirTimer--;
			}
			
			//TODO: fix float comparison
			switch (AirTimer)
			{
				case 1500f: 
				case 1200f:
				case 900f:
					if (Id != 0) break;
					//TODO: audio
					//audio_play_sfx(sfx_air_alert);
					break;
					
				case 720f:
					if (Id != 0) break;
					//TODO: audio
					//audio_play_bgm(bgm_drowning);
					break;
					
				case 0f:
					//TODO: audio
					//audio_play_sfx(sfx_drown);
					ResetState();
					
					//TODO: depth
					//depth = 50;
					Animation = Animations.Drown;
					TileLayer = Constants.TileLayers.None;
					Speed.X = 0;
					Speed.Y = 0;
					Gravity	= 0.0625f;
					IsAirLock = true;
					Camera.MainCamera.Target = null;
					return;
				
				case -1f:
					if ((int)Position.Y <= Camera.MainCamera.BufferPosition.Y + SharedData.GameHeight + 276) return;
					
					if (Id == 0)
					{
						FrameworkData.UpdateEffects = false;
						FrameworkData.UpdateObjects = false;
						FrameworkData.AllowPause = false;
					}
						
					IsDead = true;
					return;
			}
		}

		if (MathF.Floor(Position.Y) >= Stage.WaterLevel) return;
		
		if (!IsHurt && Action != Actions.Glide)
		{
			if (SharedData.PlayerPhysics <= PhysicsTypes.S2 || Speed.Y >= -4f)
			{
				Speed.Y *= 2f;
			}
					
			if (Speed.Y < -16f)
			{
				Speed.Y = -16f;
			}
					
			if (Action != Actions.Flight)
			{
				Gravity = GravityType.Default;
			}
		}
			
		//TODO: audio
		/*
			if (Action == Actions.Flight)
			{
				audio_play_sfx(sfx_flight, true);
			}
				
			if (audio_is_playing(bgm_drowning))
			{
				stage_reset_bgm();
			}
			*/
				
		IsUnderwater = false;	
		AirTimer = Constants.AirValueMax;
			
		ProcessWaterSplash();
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
			if (IsGrounded && SharedData.RotationMode > 0)
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
				VisualAngle = Mathf.RadToDeg(MathF.Atan2(
					Mathf.DegToRad(MathF.Sin(radians) + MathF.Sin(radiansVisual) * step), 
					Mathf.DegToRad(MathF.Cos(radians) + MathF.Cos(radiansVisual) * step)));
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
	
	private void ProcessPalette()
	{
		int[] colours = Type switch
		{
			Types.Tails => [4, 5, 6],
			Types.Knuckles => [7, 8, 9],
			Types.Amy => [10, 11, 12],
			_ => [0, 1, 2, 3]
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
	
	#region Animation

	private void ProcessAnimate()
	{
		if (FrameworkData.UpdateObjects)
		{
			if (AnimationBuffer == Animations.None && AnimationTimer > 0f)
			{
				AnimationBuffer = Animation;
			}
		
			if (AnimationTimer < 0)
			{
				if (Animation == AnimationBuffer)
				{
					Animation = Animations.Move;
				}
			
				AnimationTimer = 0;
				AnimationBuffer = Animations.None;
			}
			else if (AnimationBuffer != Animations.None)
			{
				AnimationTimer--;
			}
		}
	
		if (Animation != Animations.Spin || Mathf.IsEqualApprox(Sprite.GetTimer(), Sprite.GetDuration()))
		{
			Sprite.Scale = new Vector2(Math.Abs(Sprite.Scale.X) * (float)Facing, Sprite.Scale.Y);
		}
		
		//TODO: ANIMATIONS!!!!
		switch (Type)
		{
			case Types.Sonic when IsSuper: AnimateSuperSonic(); break;
			case Types.Sonic: AnimateSuperSonic(); break;
			case Types.Tails: AnimateTails(); break;
			case Types.Knuckles: AnimateKnuckles(); break;
			case Types.Amy: AnimateAmy(); break;
			
			case Types.None:
			case Types.Global:
			case Types.GlobalAI: break;
		}
	}

	public void AnimateSuperSonic()
	{
		
	}
	
	public void AnimateSonic()
	{
		
	}

	public void AnimateTails()
	{
		
	}
	
	public void AnimateKnuckles()
	{
		
	}
	
	public void AnimateAmy()
	{
		
	}

	#endregion
	
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
	
	private void SetInput(Buttons inputPress, Buttons inputDown)
	{
		InputPress = inputPress;
		InputDown = inputDown;
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
			case Stages.TSZ.StageTsz:
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
		
		// If in developer mode, remap debug button to SpaceBar
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

				FrameworkData.UpdateAnimations = true;
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
			if (Activator.CreateInstance(EditModeObjects[EditModeIndex]) 
			    is not CommonObject newObject) return true;
			
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
		ResetGravity();
		
		IsGrounded = true;
	
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
