using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : Framework.CommonObject.CommonObject
{
	private const byte EditModeAccelerationMultiplier = 4;
	private const float EditModeAcceleration = 0.046875f;
	private const byte EditModeSpeedLimit = 16;
	
	public static List<Player> Players { get; }
    
	[Export] public PlayerConstants.Type Type;

	public int Id { get; private set; }

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
	public bool IsPushing { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public Framework.CommonObject.CommonObject OnObject { get; set; }
	public bool IsSuper { get; set; }
	public bool IsInvincible { get; set; }
	public int SuperValue { get; set; }

	public PlayerConstants.Action Action { get; set; }
	public int ActionState { get; set; }
	public int ActionValue { get; set; }
	public int ActionValue2 { get; set; }
	public bool BarrierFlag { get; set; }
	public Constants.Barrier BarrierType { get; set; }
    
	public Constants.Direction Facing { get; set; }
	public PlayerConstants.Animation Animation { get; set; }
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
    
	public PlayerConstants.CpuState CpuState { get; set; }
	public float CpuTimer { get; set; }
	public float CpuInputTimer { get; set; }
	public bool IsCpuJumping { get; set; }
	public bool IsCpuRespawn { get; set; }
	public Player CpuTarget { get; set; }
    
	public PlayerConstants.RestartState RestartState { get; set; }
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
		Players = new List<Player>();
	}

	public override void _Ready()
	{
		SetBehaviour(BehaviourType.Unique);
        
		switch (Type)
		{
			case PlayerConstants.Type.Tails:
				RadiusNormal = new Vector2I(9, 15);
				RadiusSpin = new Vector2I(7, 14);
				break;
			case PlayerConstants.Type.Amy:
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
		BarrierType = Constants.Barrier.None;
		Facing = Constants.Direction.Positive;
		Animation = PlayerConstants.Animation.Idle;
		AirTimer = 1800f;
		CpuState = PlayerConstants.CpuState.Main;
		RestartState = PlayerConstants.RestartState.GameOver;
		InputPress = new Buttons();
		InputDown = new Buttons();
		RecordedData = new List<RecordedData>();

		if (Type == PlayerConstants.Type.Tails)
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
				BarrierType = FrameworkData.PlayerBackupData.BarrierType;
			}
		}
		
		if (Id == 0)
		{
			if (FrameworkData.SavedBarrier != Constants.Barrier.None)
			{
				BarrierType = FrameworkData.SavedBarrier;
				AddChild(new OrbinautFramework3.Objects.Spawnable.Barrier.Barrier(this));
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
			(Players[i]).Id--;
		}
		FrameworkData.CurrentScene.RemovePlayerStep(this);
		if (Players.Count == 0 || !IsCpuRespawn) return;
		var newPlayer = new Player()
		{
			Type = Type,
			Position = Players.First().Position
		};

		newPlayer.PlayerStep(FrameworkData.ProcessSpeed);
	}

	public void PlayerStep(double processSpeed)
	{
		var processSpeedF = (float)processSpeed; 
		// Process player input
		UpdateInput();

		// Process edit mode. Returns true if player is in edit mode state
		if (ProcessEditMode(processSpeedF))
		{
			ProcessPalette();
			return;
		}
	    
		// Process AI code. Returns true if player is deleted
		if (ProcessAI(processSpeedF)) return;
	    
		// Code to run if player is dead
		if (IsDead)
		{
			ProcessPosition(processSpeedF);
			ProcessRestart(processSpeedF);
		}
	    
		// Code to run if player is not dead. Runs only when object processing is enabled
		else if (FrameworkData.UpdateObjects)
		{
			// Apply physics parameters for this step
			UpdateParameters();
			
			// Run a repeat loop once, so we can exit from a sub-state if needed
			UpdatePhysics();
			
			/*
		scr_player_camera();
		scr_player_status_update();
		scr_player_water();
		scr_player_collision_update();
		scr_player_record_data();
		*/
		}
		
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
			case PlayerConstants.Action.PeelOut:
				//audio_stop_sfx(sfx_charge2);
				break;
		
			case PlayerConstants.Action.Flight:
				//audio_stop_sfx(sfx_flight);
				//audio_stop_sfx(sfx_flight2);
				break;
		}
	
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsPushing = false;
		IsGrounded = false;
		OnObject = null;
	
		StickToConvex = false;
		GroundMode = 0;
	
		Action = PlayerConstants.Action.None;
	
		Radius = RadiusNormal;
	}
    
	private void EditModeInit()
	{
		EditModeObjects = new List<Type>
		{ 
			typeof(Common.Ring.Ring), typeof(Common.GiantRing.GiantRing), typeof(Common.ItemBox.ItemBox), typeof(Common.Springs.Spring), typeof(Common.Motobug.Motobug), typeof(Common.Signpost.Signpost)
		};
	    
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

		var debugButton = false;
		
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

				Animation = PlayerConstants.Animation.Move;
				
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
			PlayerConstants.Type.Tails => new[] { 4, 5, 6 },
			PlayerConstants.Type.Knuckles => new[] { 7, 8, 9 },
			PlayerConstants.Type.Amy => new[] { 10, 11, 12 },
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
			case PlayerConstants.Type.Sonic:
				duration = colour switch
				{
					< 2 => 19,
					< 7 => 4,
					_ => 8
				};
			    
				colourLast = 16;
				colourLoop = 7;
				break;
			case PlayerConstants.Type.Tails:
				duration = colour < 2 ? 28 : 12;
				colourLast = 7;
				colourLoop = 2;
				break;
			case PlayerConstants.Type.Knuckles:
				duration = colour switch
				{
					< 2 => 17,
					< 3 => 15,
					_ => 3
				};

				colourLast = 11;
				colourLoop = 3;
				break;
			case PlayerConstants.Type.Amy:
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
				if (Type == PlayerConstants.Type.Sonic)
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
		if (Action == PlayerConstants.Action.Carried) return;

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
		if (Id > 0) return;

		switch (RestartState)
		{
			// GameOver
			case 0:
				float bound = FrameworkData.ViewSize.Y + 32f;
		
				if (FrameworkData.PlayerPhysics < PlayerConstants.PhysicsType.S3)
				{
					bound += Camera.MainCamera.LimitBottom * processSpeed;
				}
				else
				{
					bound += Camera.MainCamera.Position.Y * processSpeed;
				}
		
				if ((int)Position.Y > bound)
				{
					RestartState = PlayerConstants.RestartState.ResetLevel;
					
					FrameworkData.UpdateTimer = false;
					
					// TODO: Check this
					if (--LifeCount > 0)
					{
						// if FrameworkData.FrameCounter != 36000
						// {
						RestartTimer = 60f;
						break;
						// }
					}
					
					// TODO: audio + gameOver
					//instance_create_depth(0, 0, 0, obj_gui_gameover);				
					//audio_play_bgm(bgm_gameover);
				}
				break;
		
			// Sonic_ResetLevel
			case PlayerConstants.RestartState.ResetLevel:
				if (RestartTimer > 0)
				{
					if (--RestartTimer == 0)
					{
						RestartState = PlayerConstants.RestartState.RestartStage;
					}
					else
					{
						break;
					}
				}
				// TODO: audio_bgm_is_playing
				/*else if (!audio_bgm_is_playing())
		    {
			    RestartState = PlayerConstants.RestartState.RestartGame;	
		    }*/
				else
				{
					break;
				}
				
				// TODO: audio + fade
				//audio_stop_bgm(0.5);
				//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
				break;
		
			// Restart Stage
			case PlayerConstants.RestartState.RestartStage:
				// TODO: Fade
				/*if (c_engine.fade.state == FADE_ST_MAX)
		    {
			    FrameworkData.SavedLives = LifeCount;
			    GetTree().ReloadCurrentScene();
		    }*/
			
				break;
		
			// Restart Game
			case PlayerConstants.RestartState.RestartGame:
				// TODO: Game restart & fade
				/*
		    if (c_engine.fade.state == FADE_ST_MAX)
		    {
			    FrameworkData.collected_giant_rings = [];
			    FrameworkData.player_backup_data = [];
			    FrameworkData.checkpoint_data = [];
				
			    // TODO: this
			    if (FrameworkData.Continues > 0)
			    {
				    game_restart(); 
			    }
			    else
			    {
				    /*
				    if global.save_slot != -1
				    {
					    global.saved_lives = 3;
					    global.saved_score = 0;
					    global.continues   = 0;
						    
					    // gamedata_save(global.activeSave);
				    }
					
				    game_restart();
			    }
		    }
			*/
				break;
		}
	}
	
	private void UpdateParameters()
	{
		/*
    if (!IsUnderwater)
	{
		if (!IsSuper)
		{
			acc = 0.046875;
			acc_glide = 0.015625;
			acc_air = 0.09375;
			dec = 0.5;
			dec_roll = 0.125;
			frc	= 0.046875;
			frc_roll = 0.0234375;
			acc_top = 6;	
			acc_climb = 1;
			jump_min_vel = -4;
			jump_vel = player_type == PLAYER_KNUCKLES ? -6 : -6.5;
		}
		else
		{
			if (Type == PlayerConstants.Type.Sonic)
			{
			    acc = 0.1875;
			    acc_air = 0.375;
			    dec = 1;
			    dec_roll = 0.125;
			    frc = 0.046875;
			    frc_roll = 0.09375;
			    acc_top = 10;
			    jump_min_vel = -4;
			    jump_vel = -8;
			}
			else
			{
			    acc = 0.09375;
			    acc_air = 0.1875;
			    acc_glide = 0.046875;
			    dec = 0.75;
			    dec_roll = 0.125;
			    frc = 0.046875;
			    frc_roll = 0.0234375;
			    acc_top = 8;
			    acc_climb = 2;
			    jump_min_vel = -4;
			    jump_vel = player_type == PLAYER_KNUCKLES ? -6 : -6.5;
			}
		}
		
		if (ItemSpeedTimer > 0)
		{
			acc	= 0.09375;
			acc_air = 0.1875;
			frc = 0.09375;
			frc_roll = 0.046875;
			acc_top = 12;
		}
	}
	else
	{
		if (!IsSuper)
		{
		    acc = 0.0234375;
		    acc_air = 0.046875;
		    acc_glide = 0.015625;
		    dec = 0.25;
		    dec_roll = 0.125;
		    frc = 0.0234375;
		    frc_roll = 0.01171875;
		    acc_top = 3;
		    acc_climb = 1;
		    jump_min_vel = -2;
		    jump_vel = player_type == PLAYER_KNUCKLES ? -3 : -3.5;
		}
		else
		{
		    if (Type == PlayerConstants.Type.Sonic)
		    {
		        acc = 0.09375;
		        acc_air = 0.1875;
		        dec = 0.5;
		        dec_roll = 0.125;
		        frc = 0.046875;
		        frc_roll = 0.046875;
		        acc_top = 5;
		        jump_min_vel = -2;
		        jump_vel = -3.5;
		    }
		    else
		    {
		        acc = 0.046875;
		        acc_air = 0.09375;
		        acc_glide = 0.046875;
		        dec = 0.375;
		        dec_roll = 0.125;
		        frc = 0.046875;
		        frc_roll = 0.0234375;
		        acc_top = 4;
		        acc_climb = 2;
		        jump_min_vel = -2;
		        jump_vel = player_type == PLAYER_KNUCKLES ? -3 : -3.5;
		    }
		}
	}
	
	if (FrameworkData.PlayerPhysics < PlayerConstants.PhysicsType.SK)
	{
		if (Type == PlayerConstants.Type.Tails)
		{
			dec_roll = dec / 4;
		}
	}
	else if (IsSuper)
	{
		frc_roll = 0.0234375;
	}
	*/
	}

	private void UpdatePhysics()
	{
		/*
    if (IsHurt)
    {
	    scr_player_level_bound();
	    scr_player_position();
	    scr_player_collision_air();
	    scr_player_land();
    }
    else if (Action != PlayerConstants.Action.ObjectControl && Action != PlayerConstants.Action.Transform)
    {
	    if (!IsGrounded)
	    {
		    if (scr_player_jump()) return;

		    scr_player_dropdash();
		    scr_player_flight();
		    scr_player_hammerspin();
		    scr_player_hammerrush();
		    scr_player_movement_air();	
		    scr_player_level_bound();
		    scr_player_position();
		    scr_player_collision_air();
		    scr_player_land();
		    scr_player_carry();
	    }
	    else if (!IsSpinning)
	    {
		    if (scr_player_spindash()) return;
			if (scr_player_peelout()) return;
			if (scr_player_jump_start()) return;

			scr_player_slope_resist();
		    scr_player_hammerrush();
		    scr_player_movement_ground();
		    scr_player_balance();
		    scr_player_collision_ground_walls();
		    scr_player_roll_start();
		    scr_player_level_bound();
		    scr_player_position();
		    scr_player_collision_ground_floor();
		    scr_player_slope_repel();
	    }
	    else
	    {
		    if (scr_player_jump_start()) return;

		    scr_player_slope_resist_roll();
		    scr_player_movement_roll();
		    scr_player_collision_ground_walls();
		    scr_player_level_bound();
		    scr_player_position();
		    scr_player_collision_ground_floor();
		    scr_player_slope_repel();
	    }
				
	    scr_player_double_spin();
    }
    */
	}
    
	public void Land()
	{
		if (!IsGrounded) return;

		ResetGravity();
	
		if (Action == PlayerConstants.Action.Flight)
		{
			//TODO: audio
			//audio_stop_sfx(sfx_flight);
			//audio_stop_sfx(sfx_flight2);
		}
		else if (Action is PlayerConstants.Action.SpinDash or PlayerConstants.Action.PeelOut)
		{
			if (Action == PlayerConstants.Action.PeelOut)
			{
				GroundSpeed = ActionValue2;
			}
		
			return;
		}
	
		if (BarrierFlag && BarrierType == Constants.Barrier.Water)
		{
			float force = IsUnderwater ? -4f : -7.5f;
			Speed = new Vector2(Mathf.Sin(Mathf.DegToRad(Angle)), Mathf.Sin(Mathf.DegToRad(Angle))) * force;

			barrier_flag = false;
			is_on_object = false;
			IsGrounded = false;
		
			with obj_barrier
			{
				if Player_Object == other.id
				{
					ani_upd_frame(0, 1, [3, 2]);
					ani_upd_duration([7, 12]);
				
					animation_timer = 20;
				}
			}
		
			audio_play_sfx(sfx_barrier_water2);
		
			exit;
		}
	
		if !is_on_object
		{
			switch animation
			{
				case ANI_IDLE:
				case ANI_DUCK:
				case ANI_HAMMERRUSH:
				case ANI_GLIDE_GRND: break;
			
				default:
				animation = ANI_MOVE;
			}
		}
		else
		{
			animation = ANI_MOVE;
		}
	
		if is_hurt
		{
			inv_frames = 120;
			gsp = 0;
		}
	
		air_lock_flag = false;
		is_spinning	= false;
		is_jumping = false;
		is_pushing = false;
		is_hurt	= false;
	
		barrier_flag = 0;
		combo_counter = 0;
	
		cpu_state = CPU_STATE_MAIN;
	
		scr_player_dropdash();
		scr_player_hammerspin();
	
		if Action != Action_HAMMERRUSH
		{
			Action = false;
		}
		else
		{
			gsp	= 6 * facing;
		}
	
		if !is_spinning
		{
			y -= radius_y_normal - radius_y;
		
			radius_x = radius_x_normal;
			radius_y = radius_y_normal; 
		}
	}
}