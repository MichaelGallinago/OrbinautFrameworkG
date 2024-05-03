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
		ProcessDeath();
		
		if (IsControlRoutineEnabled)
		{
			base._Process(delta);
		}

		if (!IsDead)
		{
			ProcessWater();
			UpdateStatus();
			UpdateCollision();
		}
		
		RecordData();
		ProcessRotation();
		Sprite.Animate(this);
		_tail?.Animate(this);
		ProcessPalette();
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
		FlickAfterGettingHit();
		
		ItemSpeedTimer = UpdateItemTimer(ItemSpeedTimer, MusicStorage.HighSpeed);
		ItemInvincibilityTimer = UpdateItemTimer(ItemInvincibilityTimer, MusicStorage.Invincibility);

		UpdateSuperForm();
		
		IsInvincible = InvincibilityTimer > 0f || ItemInvincibilityTimer > 0f || 
		               IsHurt || SuperTimer > 0f || Shield.State == ShieldContainer.States.DoubleSpin;
		
		KillPlayerOnTimeLimit();
	}
	
	private void CreateSkidDust()
	{
		if (Animation != Animations.Skid) return;
		
		//TODO: fix loop on stutter (maybe PreviousProcessSpeed?)
		if (ActionValue2 % 4f < Scene.Local.ProcessSpeed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		ActionValue2 += Scene.Local.ProcessSpeed;
	}

	private void FlickAfterGettingHit()
	{
		if (InvincibilityTimer <= 0f || IsHurt) return;
		Visible = ((int)InvincibilityTimer & 4) > 0 || InvincibilityTimer <= 0f;
		InvincibilityTimer -= Scene.Local.ProcessSpeed;
	}
	
	private float UpdateItemTimer(float timer, AudioStream itemMusic)
	{
		if (timer <= 0f) return 0f;
		timer -= Scene.Local.ProcessSpeed;
		
		if (timer > 0f) return timer;
		timer = 0f;

		if (Id == 0 && AudioPlayer.Music.IsPlaying(itemMusic))
		{
			ResetMusic();
		}
		
		return timer;
	}

	private void UpdateSuperForm()
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

		float newSuperTimer = SuperTimer - Scene.Local.ProcessSpeed;
		if (newSuperTimer > 0f)
		{
			SuperTimer = newSuperTimer;
			return;
		}

		if (--SharedData.PlayerRings > 0)
		{
			SuperTimer = 61f;
			return;
		}
		
		SharedData.PlayerRings = 0;
		InvincibilityTimer = 1;
		SuperTimer = 0f;
		
		ResetMusic();
	}

	private void KillPlayerOnTimeLimit()
	{
		if (Id == 0 && Scene.Local.Time >= 36000f)
		{
			Kill();
		}
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
		
		if ((int)Position.Y < Stage.Local.WaterLevel) return true;
		
		IsUnderwater = true;
		AirTimer = Constants.DefaultAirTimer;

		ResetGravityOnWaterEdge();

		if (PreviousPosition.Y < Stage.Local.WaterLevel)
		{
			SpawnWaterSplash();
		}
		
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		
		Velocity.Vector *= new Vector2(0.5f, 0.25f);
		
		RemoveShieldUnderwater();

		if (Action != Actions.Flight) return false;
		AudioPlayer.Sound.Stop(SoundStorage.Flight);
		AudioPlayer.Sound.Stop(SoundStorage.Flight2);
		return false;
	}

	private void ResetGravityOnWaterEdge()
	{
		if (Action != Actions.Flight && (Action != Actions.Glide || ActionState == (int)GlideStates.Fall))
		{
			ResetGravity();
		}
	}

	private void RemoveShieldUnderwater()
	{
		if (Shield.Type is not (ShieldContainer.Types.Fire or ShieldContainer.Types.Lightning)) return;
		
		if (Shield.Type == ShieldContainer.Types.Lightning)
		{
			//TODO: obj_water_flash
			//instance_create(x, y, obj_water_flash);
		}

		SharedData.PlayerShield = ShieldContainer.Types.None;
	}

	private enum AirTimerStates : byte
	{
		None, Alert1, Alert2, Alert3, Drowning, Drown
	}

	private bool UpdateAirTimer()
	{
		if (Shield.Type == ShieldContainer.Types.Bubble) return false;

		AirTimerStates previousState = GetAirTimerState(AirTimer);
		if (AirTimer > 0f)
		{
			AirTimer -= Scene.Local.ProcessSpeed;
		}

		AirTimerStates state = GetAirTimerState(AirTimer);
		if (state != previousState + 1) return false;
		
		switch (state)
		{
			case AirTimerStates.Alert1 or AirTimerStates.Alert2 or AirTimerStates.Alert3 when Id == 0:
				AudioPlayer.Sound.Play(SoundStorage.Alert);
				break;
			
			case AirTimerStates.Drowning when Id == 0:
				AudioPlayer.Music.Play(MusicStorage.Drowning);
				break;
			
			case AirTimerStates.Drown:
				ProcessDrown();
				return true;
		}

		return false;
	}

	private static AirTimerStates GetAirTimerState(float value) => value switch
	{
		> 1500f => AirTimerStates.None,
		> 1200f => AirTimerStates.Alert1,
		> 900f => AirTimerStates.Alert2,
		> 720f => AirTimerStates.Alert3,
		> 0f => AirTimerStates.Drowning,
		_ => AirTimerStates.Drown
	};

	private void ProcessDrown()
	{
		AudioPlayer.Sound.Play(SoundStorage.Drown);
		ResetState();

		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		Animation = Animations.Drown;
		IsDead = true;
		IsObjectInteractionEnabled = false;
		Gravity	= GravityType.Underwater;
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		
		if (Camera != null)
		{
			Camera.IsMovementAllowed = false;
		}
	}

	private void LeaveWater()
	{
		if ((int)Position.Y >= Stage.Local.WaterLevel) return;

		IsUnderwater = false;
		ResetGravityOnWaterEdge();
		
		if (PreviousPosition.Y >= Stage.Local.WaterLevel)
		{
			SpawnWaterSplash();
		}
		
		if (Action == Actions.Flight)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
			
		if (Id == 0 && AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			ResetMusic();
		}
		
		AccelerateOnLeavingWater();
	}

	private void AccelerateOnLeavingWater()
	{
		if (SharedData.PlayerPhysics <= PhysicsTypes.S2 || Velocity.Y >= -4f)
		{
			Velocity.Y *= 2f;
		}
					
		if (Velocity.Y < -16f)
		{
			Velocity.Y = -16f;
		}
	}

	private void SpawnWaterSplash()
	{
		if (Action is Actions.Climb or Actions.Glide || CpuState == CpuStates.Respawn) return;
		
		//TODO: obj_water_splash
		//instance_create(x, c_stage.water_level, obj_water_splash);
		AudioPlayer.Sound.Play(SoundStorage.Splash);
	}

	private void UpdateCollision()
	{
		SetSolid(RadiusNormal.X + 1, Radius.Y);
		SetRegularHitBox();
		SetExtraHitBox();
	}

	private void SetRegularHitBox()
	{
		if (Animation != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitBox(8, Radius.Y - 3);
			return;
		}

		if (Type is Types.Tails or Types.Amy) return;
		SetHitBox(8, 10, 0, 6);
	}

	private void SetExtraHitBox()
	{
		switch (Animation)
		{
			case Animations.HammerSpin:
				SetHitBoxExtra(25, 25);
				break;
			
			case Animations.HammerDash:
				(int radiusX, int radiusY, int offsetX, int offsetY) = (Sprite.Frame & 3) switch
				{
					0 => (16, 16,  6,  0),
					1 => (16, 16, -7,  0),
					2 => (14, 20, -4, -4),
					3 => (17, 21,  7, -5),
					_ => throw new ArgumentOutOfRangeException()
				};
				SetHitBoxExtra(radiusX, radiusY, offsetX * (int)Facing, offsetY);
				break;
			default:
				SetHitBoxExtra(Shield.State == ShieldContainer.States.DoubleSpin ? 
					new Vector2I(24, 24) : Vector2I.Zero);
				break;
		}
	}

	private void ProcessRotation()
	{
		bool isSmoothRotation = SharedData.RotationMode > 0;
		
		if (!IsGrounded)
		{
			VisualAngle = Angle;
		}
		else if (isSmoothRotation)
		{
			RotateOnGround();
		}
		else
		{
			VisualAngle = Angle is > 22.5f and < 337.5f ? Angle : 0;
		}

		if (!isSmoothRotation)
		{
			VisualAngle = MathF.Ceiling((VisualAngle - 22.5f) / 45f) * 45f;
		}

		RotationDegrees = Animation == Animations.Move ? VisualAngle : 0f;
	}

	private void RotateOnGround()
	{
		// Ground smooth rotation code by Nihil
		var angle = 0f;
		float step;
		
		if (Angle is > 22.5f and < 337.5f)
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
	
	private void ProcessPalette()
	{
		// Get player colour IDs
		ReadOnlySpan<int> colours = PlayerColourIds;
		
		int colour = PaletteUtilities.Index[colours[0]];
		UpdateSuperPalette(colour, out int colourLast, out int colourLoop, out int duration);
		UpdateRegularPalette(colour, ref colourLoop, ref colourLast, ref duration);
		
		// Apply palette
		PaletteUtilities.SetRotation(colours, colourLoop, colourLast, duration);
	}

	private ReadOnlySpan<int> PlayerColourIds => Type switch
	{
		Types.Tails => [4, 5, 6],
		Types.Knuckles => [7, 8, 9],
		Types.Amy => [10, 11, 12],
		_ => [0, 1, 2, 3]
	};

	private void UpdateSuperPalette(int colour, out int colourLast, out int colourLoop, out int duration)
	{
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
			
			default:
				duration = 0;
				colourLast = 0;
				colourLoop = 0;
				break;
		}
	}

	private void UpdateRegularPalette(int colour, ref int colourLoop, ref int colourLast, ref int duration)
	{
		if (SuperTimer > 0f) return;
		
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

	public void IncreaseComboScore(int comboCounter = 0)
	{
		SharedData.ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
	}

	private void ProcessDeath()
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

		SetNextStateOnDeath();
	}

	protected virtual void SetNextStateOnDeath()
	{
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

	//TODO: update debug mode
	public void OnEnableEditMode()
	{
		ResetGravity();
		ResetState();
		//ResetZIndex();
				
		Visible = true;
		IsObjectInteractionEnabled = false;
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
