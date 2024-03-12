using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : PhysicalPlayerWithAbilities, IEditor, IAnimatedPlayer, ITailed
{
	private const byte MaxRecordLength = 32;
	private readonly EditMode _editMode = new();
	
	[Export] public PackedScene PackedTail { get; private set; }
	[Export] public PlayerAnimatedSprite Sprite { get; private set; }
	private Tail _tail;

	public Player() => TypeChanged += OnTypeChanged;
	
	public override void _Ready()
	{
		base._Ready();
		if (GetOwner<Node>() is CommonScene scene)
		{
			TileMap = scene.CollisionTileMap;
			if (scene is CommonStage stage)
			{
				Stage = stage;
			}
		}
		
		Sprite.FrameChanged += () => IsAnimationFrameChanged = true;
	}

	public override void Reset()
	{
		base.Reset();
		Sprite.Animate(this);
	}
	
	private new void QueueFree()
	{
		Players.Remove(this);
		for (int i = Id; i < Players.Count; i++)
		{
			Players[i].Id--;
		}
		base.QueueFree();
	}

	public override void _ExitTree()
	{
		Players.Remove(this);
		for (int i = Id; i < Players.Count; i++)
		{
			Players[i].Id--;
		}
		base._ExitTree();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Id = Players.Count;
		Players.Add(this);
	}
	
	public override void _Process(double delta)
	{
		if (FrameworkData.IsPaused || !FrameworkData.UpdateObjects && !IsDead) return;
		
		float processSpeed = FrameworkData.ProcessSpeed;
		
		// Process local input
		Input.Update(Id);

		// Process Edit Mode
		if (Id == 0)
		{
			if (_editMode.Update(processSpeed, this, Input)) return;
		}
	    
		// Process CPU Player logic (return if flying in or respawning)
		if (ProcessCpu(processSpeed)) return;
	    
		// Process Restart Event
		ProcessRestart(processSpeed);
	    
		// Process default player control routine
		base._Process(delta);

		Camera.Main?.UpdatePlayerCamera(this);
		UpdateStatus();
		ProcessWater();
		RecordData();
		ProcessRotation();
		Sprite.Animate(this);
		_tail?.Animate(this);
		ProcessPalette();
		UpdateCollision();
	}
	
	public void ResetMusic()
	{
		if (IsSuper)
		{
			AudioPlayer.PlayMusic(MusicStorage.Super);
		}
		else if (ItemInvincibilityTimer > 0f)
		{
			AudioPlayer.PlayMusic(MusicStorage.Invincibility);
		}
		else if (ItemSpeedTimer > 0f)
		{
			AudioPlayer.PlayMusic(MusicStorage.HighSpeed);
		}
		else if (Stage.Music != null)
		{
			AudioPlayer.PlayMusic(Stage.Music);
		}
	}
	
	protected virtual bool ProcessCpu(float processSpeed) => false;

	private void OnTypeChanged(Types newType)
	{
		switch (newType)
		{
			case Types.Tails:
				if (_tail != null) return;
				_tail = PackedTail.Instantiate<Tail>();
				AddChild(_tail);
				break;
			
			case Types.Knuckles:
				ClimbAnimationFrameNumber = Sprite.GetAnimationFrameCount(Animations.ClimbWall, newType);
				RemoveTail();
				break;
			
			default:
				RemoveTail();
				break;
		}
	}

	private void RemoveTail()
	{
		if (_tail == null) return;
		_tail.QueueFree();
		_tail = null;
	}
	
	private void UpdateStatus()
	{
		if (IsDead) return;

		CreateSkidDust();
		UpdateInvincibilityFlashes();

		ItemSpeedTimer = UpdateItemTimer(ItemSpeedTimer);
		ItemInvincibilityTimer = UpdateItemTimer(ItemInvincibilityTimer);

		UpdateSuperStatus();
		
		IsInvincible = InvincibilityTimer != 0 || ItemInvincibilityTimer != 0 || 
		               IsHurt || IsSuper || Barrier.State == Barrier.States.DoubleSpin;
		
		if (Id == 0 && FrameworkData.Time >= 36000d)
		{
			Kill();
		}
		
		UpdateLiveRewards();
	}

	private void UpdateInvincibilityFlashes()
	{
		if (InvincibilityTimer <= 0f) return;
		Visible = ((int)InvincibilityTimer & 4) >= 1 || InvincibilityTimer == 0f;
		InvincibilityTimer -= FrameworkData.ProcessSpeed;
	}

	private void CreateSkidDust()
	{
		if (Animation != Animations.Skid) return;
		
		if (ActionValue2 % 4f < FrameworkData.ProcessSpeed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		ActionValue2 += FrameworkData.ProcessSpeed;
	}

	private static float UpdateItemTimer(float timer)
	{
		if (timer <= 0f) return timer;
		timer -= FrameworkData.ProcessSpeed;
		if (timer <= 0f)
		{
			Players[0].ResetMusic();
		}

		return timer;
	}

	private void UpdateSuperStatus()
	{
		if (!IsSuper) return;
		
		if (Action == Actions.Transform)
		{
			ActionValue -= FrameworkData.ProcessSpeed;
			if (ActionValue <= 0f)
			{
				ObjectInteraction = true;
				Action = Actions.None;
			}
		}

		if (SuperValue > 0f)
		{
			SuperValue -= FrameworkData.ProcessSpeed;
			return;
		}

		if (--RingCount > 0)
		{
			SuperValue = 60f;
			return;
		}
		
		RingCount = 0;
		InvincibilityTimer = 1;
		IsSuper = false;
				
		Players[0].ResetMusic();
	}

	private void UpdateLiveRewards()
	{
		if (Id != 0 || LifeRewards.Count <= 0)
		{
			LifeRewards = [RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000];
			return;
		}
		
		if (RingCount >= LifeRewards[0] && LifeRewards[0] <= 200)
		{
			LifeCount++;
			LifeRewards[0] += 100;

			//TODO: audio
			//AudioPlayer.PlaySound(SoundStorage.ExtraLife);
		}

		if (ScoreCount < LifeRewards[1]) return;
		LifeCount++;
		LifeRewards[1] += 50000;

		//TODO: audio
		//AudioPlayer.PlaySound(SoundStorage.ExtraLife);
	}

	private void ProcessWater()
	{
		if (IsDead || Stage is not { IsWaterEnabled: true }) return;
		
		if (DiveIntoWater()) return;
		if (UpdateAirTimer()) return;
		LeaveWater();
	}

	private bool DiveIntoWater()
	{
		if (IsUnderwater) return false;
		
		if (Mathf.Floor(Position.Y) < Stage.WaterLevel) return true;
		
		IsUnderwater = true;
		
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		ProcessWaterSplash();
		SlowDownOnDive();
		RemoveBarrierUnderwater();

		if (Action != Actions.Flight) return true;
		AudioPlayer.StopSound(SoundStorage.Flight);
		AudioPlayer.StopSound(SoundStorage.Flight2);

		return true;
	}

	private void SlowDownOnDive()
	{
		if (IsHurt) return;
		
		if (Action != Actions.Flight && (Action != Actions.Glide || (GlideStates)ActionState == GlideStates.Fall))
		{
			Gravity = GravityType.Underwater;
		}
			
		Velocity.Vector *= new Vector2(0.5f, 0.25f);
	}

	private void RemoveBarrierUnderwater()
	{
		if (Barrier.Type is not (Barrier.Types.Flame or Barrier.Types.Thunder)) return;
		
		if (Barrier.Type == Barrier.Types.Thunder)
		{
			//TODO: obj_water_flash
			//instance_create(x, y, obj_water_flash);
		}
			
		Barrier.Type = Barrier.Types.None;
	}

	private bool UpdateAirTimer()
	{
		if (Barrier.Type == Barrier.Types.Water) return false;
		
		if (AirTimer > -1f)
		{
			AirTimer -= FrameworkData.ProcessSpeed;
		}
			
		//TODO: fix float comparison
		switch (AirTimer)
		{
			case 1500f:
			case 1200f:
			case 900f:
				if (Id != 0) break;
				//TODO: audio
				//AudioPlayer.PlaySound(SoundStorage.AirAlert);
				break;
					
			case 720f:
				if (Id != 0) break;
				AudioPlayer.PlayMusic(MusicStorage.Drowning);
				break;
					
			case 0f:
				AudioPlayer.PlaySound(SoundStorage.Drown);
				ResetState();

				ZIndex = 0;
				Animation = Animations.Drown;
				TileLayer = Constants.TileLayers.None;
				Velocity.Vector = Vector2.Zero;
				Gravity	= 0.0625f;
				IsAirLock = true;
				if (Camera.Main == null) return true;
				Camera.Main.Target = null;
				return true;
				
			case -1f:
				if (Camera.Main == null) return true;
				if ((int)Position.Y <= Camera.Main.BufferPosition.Y + SharedData.ViewSize.Y + 276) return true;
					
				if (Id == 0)
				{
					FrameworkData.UpdateEffects = false;
					FrameworkData.UpdateObjects = false;
					FrameworkData.AllowPause = false;
				}
						
				IsDead = true;
				return true;
		}

		return false;
	}

	private void LeaveWater()
	{
		if (MathF.Floor(Position.Y) >= Stage.WaterLevel) return;

		AccelerateOnLeavingWater();
		
		if (Action == Actions.Flight)
		{
			AudioPlayer.PlaySound(SoundStorage.Flight);
		}
			
		if (AudioPlayer.CheckMusicPlaying(MusicStorage.Drowning))
		{
			Players[0].ResetMusic();
		}
				
		IsUnderwater = false;	
		AirTimer = Constants.MaxAirValue;
			
		ProcessWaterSplash();
	}

	private void AccelerateOnLeavingWater()
	{
		if (IsHurt || Action == Actions.Glide) return;
		
		if (SharedData.PlayerPhysics <= PhysicsTypes.S2 || Velocity.Y >= -4f)
		{
			Velocity.Y *= 2f;
		}
					
		if (Velocity.Y < -16f)
		{
			Velocity.Y = -16f;
		}
					
		if (Action != Actions.Flight)
		{
			Gravity = GravityType.Default;
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
		//AudioPlayer.PlaySound(SoundStorage.WaterSplash);
	}

	private void UpdateCollision()
	{
		if (IsDead) return;
		
		SetSolid(new Vector2I(RadiusNormal.X + 1, Radius.Y));
		SetRegularHitbox();
		SetExtraHitbox();
	}

	private void SetRegularHitbox()
	{
		if (Animation != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitbox(new Vector2I(8, Radius.Y - 3));
			return;
		}

		if (Type is Types.Tails or Types.Amy) return;
		SetHitbox(new Vector2I(8, 10), new Vector2I(0, 6));
	}

	private void SetExtraHitbox()
	{
		switch (Animation)
		{
			case Animations.HammerSpin:
				SetHitboxExtra(new Vector2I(25, 25));
				break;
			
			case Animations.HammerDash:
				//TODO: replace by methods overloading
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
				break;
			
			default:
				SetHitboxExtra(Barrier.State == Barrier.States.DoubleSpin ? new Vector2I(24, 24) : Vector2I.Zero);
				break;
		}
	}

	private void RecordData()
	{
		if (IsDead) return;
		
		RecordedData.Add(new RecordedData(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy));
		if (RecordedData.Count <= MaxRecordLength) return;
		RecordedData.RemoveAt(0);
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
			RotationDegrees = 360f - VisualAngle;
		}
		else
		{
			RotationDegrees = 360f - MathF.Ceiling((VisualAngle - 22.5f) / 45f) * 45f;
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

	public void IncreaseComboScore(int comboCounter = 0)
	{
		ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
	}
    
	public override void ResetState()
	{
		switch (Action)
		{
			case Actions.PeelOut:
				AudioPlayer.StopSound(SoundStorage.Charge2);
				break;
		
			case Actions.Flight:
				AudioPlayer.StopSound(SoundStorage.Flight);
				AudioPlayer.StopSound(SoundStorage.Flight2);
				break;
		}
		base.ResetState();
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

				if (Camera.Main != null)
				{
					//TODO: check processSpeed
					if (FrameworkData.PlayerPhysics < PhysicsTypes.S3)
					{
						bound += Camera.Main.LimitBottom * processSpeed; // TODO: check if LimitBottom or Bounds
					}
					else
					{
						bound += Camera.Main.BufferPosition.Y * processSpeed + SharedData.ViewSize.Y;
					}
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
					
					// TODO: gameOver
					//instance_create_depth(0, 0, 0, obj_gui_gameover);		
					AudioPlayer.PlayMusic(MusicStorage.GameOver);
				}
				break;
		
			// If RestartTimer was set
			case RestartStates.ResetLevel:
				if (RestartTimer > 0)
				{
					if (--RestartTimer != 0) break;
					RestartState = RestartStates.RestartStage;
				}
				else if (!AudioPlayer.CheckMusicPlaying(null))
				{
					break;
				}
				
				// If restart_timer wasn't set (Game Over or Time Over)
				RestartState = RestartStates.RestartGame;	
				AudioPlayer.StopMusic(0.5f);
				
				// TODO: fade
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

	public void OnEnableEditMode()
	{
		ResetGravity();
		ResetState();
		ResetZIndex();
				
		ObjectInteraction = false;
		Visible = true;
	}

	public void OnDisableEditMode()
	{
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		Animation = Animations.Move;
		ObjectInteraction = true;
		IsDead = false;
	}
}
