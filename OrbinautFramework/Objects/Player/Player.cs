using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : PhysicalPlayerWithAbilities, IEditor, IAnimatedPlayer, ITailed, ICameraHolder
{
	private readonly DebugMode _debugMode = new();
	
	[Export] public PackedScene PackedTail { get; private set; }
	[Export] public PlayerAnimatedSprite Sprite { get; private set; }
	private Tail _tail;

	public Player() => TypeChanged += OnTypeChanged;
	
	public override void _Ready()
	{
		base._Ready();
		TileMap = Scene.Local.CollisionTileMap;
		Sprite.FrameChanged += () => IsAnimationFrameChanged = true;
	}

	protected override void Init()
	{
		base.Init();
		Sprite.Animate(this);
	}

	public override void _ExitTree()
	{
		RemovePlayer();
		base._ExitTree();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Players.Add(this);
	}
	
	public override void _Process(double delta)
	{
		if (Scene.Local.IsPaused && DeathState == DeathStates.Wait) return;
		
		float processSpeed = Scene.Local.ProcessSpeed;
		
		Input.Update(Id);

		// DEBUG MODE PLAYER ROUTINE
		if (DeathState == DeathStates.Wait && Id == 0 && SharedData.IsDebugModeEnabled)
		{
			if (_debugMode.Update(processSpeed, this, Input)) return;
		}
	    
		// DEFAULT PLAYER ROUTINE
		ProcessCpu(processSpeed);
		ProcessDeath(processSpeed);
		
		if (IsControlRoutineEnabled)
		{
			base._Process(delta);
		}
		
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
		if (SuperTimer > 0f)
		{
			AudioPlayer.Music.Play(MusicStorage.Super);
		}
		else if (ItemInvincibilityTimer > 0f)
		{
			AudioPlayer.Music.Play(MusicStorage.Invincibility);
		}
		else if (ItemSpeedTimer > 0f)
		{
			AudioPlayer.Music.Play(MusicStorage.HighSpeed);
		}
		else if (Stage.Local != null && Stage.Local.Music != null)
		{
			AudioPlayer.Music.Play(Stage.Local.Music);
		}
	}
	
	protected virtual void ProcessCpu(float processSpeed) {}
	
	private void RemovePlayer()
	{
		Players.Remove(this);
		for (int i = Id; i < Players.Count; i++)
		{
			Players[i].Id--;
		}
	}

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
		CreateSkidDust();
		UpdateInvincibilityFlashes();
		
		ItemSpeedTimer = UpdateItemTimer(ItemSpeedTimer, MusicStorage.HighSpeed);
		ItemInvincibilityTimer = UpdateItemTimer(ItemInvincibilityTimer, MusicStorage.Invincibility);

		UpdateSuperStatus();
		
		IsInvincible = InvincibilityTimer > 0f || ItemInvincibilityTimer > 0f || 
		               IsHurt || SuperTimer > 0f || Shield.State == ShieldContainer.States.DoubleSpin;
		
		if (Id == 0 && Scene.Local.Time >= 36000d)
		{
			Kill();
		}
	}

	private void UpdateInvincibilityFlashes()
	{
		if (InvincibilityTimer <= 0f || IsHurt) return;
		Visible = ((int)InvincibilityTimer & 4) >= 1 || InvincibilityTimer == 0f;
		InvincibilityTimer -= Scene.Local.ProcessSpeed;
	}

	private void CreateSkidDust()
	{
		if (Animation != Animations.Skid) return;
		
		if (ActionValue2 % 4f < Scene.Local.ProcessSpeed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		ActionValue2 += Scene.Local.ProcessSpeed;
	}

	private float UpdateItemTimer(float timer, AudioStream itemMusic)
	{
		if (timer <= 0f) return 0f;
		timer -= Scene.Local.ProcessSpeed;
		
		if (timer > 0f) return timer;
		timer = 0f;

		if (AudioPlayer.Music.IsPlaying(itemMusic))
		{
			ResetMusic();
		}
		
		return timer;
	}

	private void UpdateSuperStatus()
	{
		if (SuperTimer <= 0f) return;
		
		if (Action == Actions.Transform)
		{
			ActionValue -= Scene.Local.ProcessSpeed;
			if (ActionValue <= 0f)
			{
				IsObjectInteractionEnabled = true;
				IsControlRoutineEnabled = true;
				Action = Actions.None;
			}
		}

		if (SuperTimer > 0f)
		{
			SuperTimer -= Scene.Local.ProcessSpeed;
			return;
		}

		if (--SharedData.PlayerRings > 0)
		{
			SuperTimer = 60f;
			return;
		}
		
		SharedData.PlayerRings = 0;
		InvincibilityTimer = 1;
		SuperTimer = 0f;
		
		ResetMusic();
	}

	private void ProcessWater()
	{
		if (IsHurt || Stage.Local == null || !Stage.Local.IsWaterEnabled) return;
		
		if (DiveIntoWater()) return;
		if (UpdateAirTimer()) return;
		LeaveWater();
	}

	private bool DiveIntoWater()
	{
		if (IsUnderwater) return false;
		
		if (Mathf.Floor(Position.Y) < Stage.Local.WaterLevel) return true;
		
		IsUnderwater = true;
		AirTimer = Constants.DefaultAirTimer;
		
		ProcessWaterSplash();
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		SlowDownOnDive();
		RemoveBarrierUnderwater();

		if (Action != Actions.Flight) return true;
		AudioPlayer.Sound.Stop(SoundStorage.Flight);
		AudioPlayer.Sound.Stop(SoundStorage.Flight2);

		return true;
	}

	private void SlowDownOnDive()
	{
		if (Action != Actions.Flight && (Action != Actions.Glide || ActionState == (int)GlideStates.Fall))
		{
			Gravity = GravityType.Underwater;
		}
			
		Velocity.Vector *= new Vector2(0.5f, 0.25f);
	}

	private void RemoveBarrierUnderwater()
	{
		if (Shield.Type is not (ShieldContainer.Types.Flame or ShieldContainer.Types.Thunder)) return;
		
		if (Shield.Type == ShieldContainer.Types.Thunder)
		{
			//TODO: obj_water_flash
			//instance_create(x, y, obj_water_flash);
		}
		
		Shield.Type = ShieldContainer.Types.None;
	}

	private bool UpdateAirTimer()
	{
		if (Shield.Type == ShieldContainer.Types.Water) return false;
		
		if (AirTimer > -1f)
		{
			AirTimer -= Scene.Local.ProcessSpeed;
		}
			
		//TODO: fix float comparison
		switch (AirTimer)
		{
			case 1500f:
			case 1200f:
			case 900f:
				if (Id != 0) break;
				AudioPlayer.Sound.Play(SoundStorage.Alert);
				break;
					
			case 720f:
				if (Id != 0) break;
				AudioPlayer.Music.Play(MusicStorage.Drowning);
				break;
					
			case 0f:
				AudioPlayer.Sound.Play(SoundStorage.Drown);
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
		}

		return false;
	}

	private void LeaveWater()
	{
		if (MathF.Floor(Position.Y) >= Stage.WaterLevel) return;

		AccelerateOnLeavingWater();
		
		if (Action == Actions.Flight)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
			
		if (AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			Players[0].ResetMusic();
		}
				
		IsUnderwater = false;	
		AirTimer = Constants.DefaultAirTimer;
			
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
		if (Action is Actions.Climb or Actions.Glide || CpuState == CpuStates.Respawn) return;
		
		//TODO: obj_water_splash
		//instance_create(x, c_stage.water_level, obj_water_splash);
		AudioPlayer.Sound.Play(SoundStorage.Splash);
	}

	private void UpdateCollision()
	{
		if (DeathState == DeathStates.Wait) return;
		
		SetSolid(new Vector2I(RadiusNormal.X + 1, Radius.Y));
		SetRegularHitBox();
		SetExtraHitBox();
	}

	private void SetRegularHitBox()
	{
		if (Animation != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitBox(new Vector2I(8, Radius.Y - 3));
			return;
		}

		if (Type is Types.Tails or Types.Amy) return;
		SetHitBox(new Vector2I(8, 10), new Vector2I(0, 6));
	}

	private void SetExtraHitBox()
	{
		switch (Animation)
		{
			case Animations.HammerSpin:
				SetHitBoxExtra(new Vector2I(25, 25));
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
				SetHitBoxExtra(radius, offset);
				break;
			
			default:
				SetHitBoxExtra(Shield.State == ShieldContainer.States.DoubleSpin ?
					new Vector2I(24, 24) : Vector2I.Zero);
				break;
		}
	}

	private void RecordData()
	{
		if (DeathState == DeathStates.Restart) return;
		
		RecordedData.Add(new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy));
		if (RecordedData.Count <= MinimalRecordLength) return;
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
		SharedData.ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
	}
    
	//TODO: what is this????????
	public override void ResetState()
	{
		switch (Action)
		{
			case Actions.PeelOut:
				AudioPlayer.Sound.Stop(SoundStorage.Charge2);
				break;
		
			case Actions.Flight:
				AudioPlayer.Sound.Stop(SoundStorage.Flight);
				AudioPlayer.Sound.Stop(SoundStorage.Flight2);
				break;
		}
		base.ResetState();
	}

	private void ProcessDeath(float processSpeed)
	{
		if (!IsDead) return;
		
		// If drowned, wait until we're far enough off-screen
		const int drownScreenOffset = 276;
		if (AirTimer == 0 && (int)Position.Y <= Camera.BufferPosition.Y + SharedData.ViewSize.Y + drownScreenOffset)
		{
			return;
		}
		
		// Stop all objects
		if (Id == 0)
		{
			Scene.Local.UpdateObjects = false;
		}
		
		switch (DeathState)
		{
			case DeathStates.Wait: WaitOnDeath(); break;
			case DeathStates.Restart: RestartOnDeath(); break;
		}
	}

	private void WaitOnDeath()
	{
		var bound = 32f;

		//TODO: check processSpeed
		if (SharedData.PlayerPhysics < PhysicsTypes.S3)
		{
			bound += Camera.Limit.W; // TODO: check if LimitBottom or Bounds
		}
		else
		{
			bound += Camera.BufferPosition.Y + SharedData.ViewSize.Y;
		}

		if ((int)Position.Y <= bound) return;
		
		RestartState = RestartStates.ResetLevel;
		
		// If CPU, respawn
		if (Id != 0)
		{
			m_player_cpu_respawn();
			return;
		}
		
		// If lead player, go to the next state
		//TODO: gui hud
		/*if (instance_exists(obj_gui_hud))
		{
			obj_gui_hud.update_timer = false;
		}*/
				
		Scene.Local.AllowPause = false;
					
		if (--SharedData.LifeCount > 0 && Scene.Local.Time < 36000f)
		{
			DeathState = DeathStates.Restart;
			RestartTimer = 60f;
		}
		else
		{
			DeathState = DeathStates.Wait;
				
			//TODO: gui gameover
			//instance_create_depth(0, 0, RENDERER_DEPTH_HUD, obj_gui_gameover);				
			AudioPlayer.Music.Play(MusicStorage.GameOver);
		}
	}

	private void RestartOnDeath()
	{
		// Wait 60 steps, then restart
		if (RestartTimer > 0f)
		{
			RestartTimer -= Scene.Local.ProcessSpeed;
			if (RestartTimer > 0f) return;
			AudioPlayer.Music.StopAllWithMute(0.5f);
					
			// TODO: fade
			//fade_perform(FADE_MD_OUT, FADE_BL_BLACK, 1);
		}
				
		// TODO: fade
		//if (c_framework.fade.state != FADESTATE.PLAINCOLOUR) break;

		Scene.Local.Tree.ReloadCurrentScene();
	}

	public void OnEnableEditMode()
	{
		ResetGravity();
		ResetState();
		ResetZIndex();
				
		IsObjectInteractionEnabled = false;
		Visible = true;
	}

	public void OnDisableEditMode()
	{
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		Animation = Animations.Move;
		IsObjectInteractionEnabled = true;
		DeathState = DeathStates.Wait;
	}
}
