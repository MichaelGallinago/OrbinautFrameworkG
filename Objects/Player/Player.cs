using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;
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
	// byte PlayerCount = instance_number(global.player_obj)
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

	public enum GlideState : sbyte
	{
		Left = -1,
		Right = 1,
		Ground = 2
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
		Glide,
		GlideFall,
		GlideGround,
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
	
	public static List<Player> Players { get; }
    
	[Export] public Types Type;

	public int Id { get; private set; }
	
	public PhysicParams PhysicParams { get; set; }
	public Vector2I Radius { get; set; }
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public Vector2 Speed { get; set; }
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
	public int ActionValue { get; set; }
	public int ActionValue2 { get; set; }
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
	    
		// Process CPU Player logic (exit if flying in or respawning)
		if (ProcessAI(processSpeedF)) return;
	    
		// Process Restart Event
		ProcessRestart(processSpeedF);
	    
		// Process default player control routine
		
		// Run a repeat loop once, so we can exit from a sub-state if needed
		UpdatePhysics();
		
		ProcessPlayerCamera();
		UpdatePlayerStatus();
		scr_player_water();
		scr_player_collision_update();
		scr_player_record_data();
		
		// Always animate player
		//scr_player_rotation();
		//scr_player_animate();
		
		// Always update player palette rotations
		ProcessPalette();
	}
    
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
		GroundMode = 0;
	
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
			EditModeSpeed = Mathf.Min(EditModeSpeed + (FrameworkData.DeveloperMode ? 
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
				Mathf.Clamp(Speed.X, -16f, 16f), 
				Mathf.Clamp(Speed.Y, -16f, 16f));
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
					    
				    // game_save_data(global.current_save_slot);
			    }
				
				//TODO: game_restart
			    //game_restart();
				break;
		}
	}

	private void UpdatePhysics()
	{
		if (Action == ACTION_OBJ_CONTROL || Action == Actions.Transform) return;

		// Define physics for this step
		PhysicParams = PhysicParams.Get(IsUnderwater, IsSuper, Type, ItemSpeedTimer);

		
		// Abilities logic
		if scr_player_spindash() break;
		if scr_player_peelout() break;
		if scr_player_jump() break;
		if scr_player_jump_start() break;
		scr_player_dropdash();
		scr_player_flight();
		scr_player_climb();
		scr_player_glide();
		scr_player_hammerspin();
		scr_player_hammerrush();
		
		// Core player logic
		scr_player_slope_resist();
		scr_player_slope_resist_roll();
		scr_player_movement_ground();
		scr_player_movement_ground_roll();
		scr_player_movement_air();
		scr_player_balance();
		scr_player_collision_ground_walls();
		scr_player_roll_start();
		scr_player_level_bound();
		scr_player_position();
		scr_player_collision_ground_floor();
		scr_player_slope_repel();
		scr_player_collision_air();
		
		// Late abilities logic
		scr_player_glide_collision();
		scr_player_carry();
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
			Speed = new Vector2(Mathf.Sin(Mathf.DegToRad(Angle)), Mathf.Sin(Mathf.DegToRad(Angle))) * force;

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
				GroundSpeed = Mathf.Floor(GroundSpeed / 4f) - force;
				if (GroundSpeed < -maxSpeed)
				{
					GroundSpeed = -maxSpeed;
				}
			}
			else if (Angle != 360)
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
			if (Angle >= 0)
			{
				GroundSpeed = Mathf.Floor(GroundSpeed / 4f) + force;
				if (GroundSpeed > maxSpeed)
				{
					GroundSpeed = maxSpeed;
				}
			}
			else if (Angle != 360)
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
		
		if (!FrameworkData.CDCamera)
		{
			Camera.MainCamera.UpdateDelay(8);
		}
		
		Vector2 dustPosition = Position;
		dustPosition.Y += Radius.Y;
		Constants.Direction facing = Facing;
		
		AddChild(new DropDashDust
		{
			Position = dustPosition,
			Scale = new Vector2((float)facing, Scale.Y)
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

	private void ProcessPlayerCamera()
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

	private void UpdatePlayerStatus()
	{
		if (IsDead) return;

		// TODO: find a better place for this (and make obj_dust_skid)
		if (Animation == Animations.Skid && AnimationTimer % 4 == 0)
		{
			//instance_create(x, y + radius_y, obj_dust_skid);
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
	
		IsInvincible = InvincibilityFrames != 0 || ItemInvincibilityTimer != 0 || IsHurt || IsSuper || barrier_state == BARRIER_STATE_DOUBLESPIN;
				 
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
}