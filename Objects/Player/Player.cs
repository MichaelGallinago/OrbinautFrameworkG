using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public partial class Player : PlayerData
{
	private readonly PlayerInput _input = new();
	private readonly EditMode _editMode = new();
	
	public override void _Ready()
	{
		Type = SpawnType switch
		{
			SpawnTypes.Global => SharedData.PlayerType,
			SpawnTypes.GlobalAI => SharedData.PlayerTypeCpu,
			_ => Type
		};

		if (Type == Types.None)
		{
			QueueFree();
			return;
		}

		
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
		Sprite.AnimationType = Animations.Idle;
		AirTimer = 1800f;
		CpuState = CpuStates.Main;
		RestartState = RestartStates.GameOver;
		_input.Clear();
		RecordedData = [];

		if (Type == Types.Tails)
		{
			Tail = PackedTail.Instantiate<Tail>();
			AddChild(Tail);
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

	private new void QueueFree()
	{
		Players.Remove(this);
		for (int i = Id; i < Players.Count; i++)
		{
			Players[i].Id--;
		}
		base.QueueFree();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Id = Players.Count;
		Players.Add(this);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		if (Players.Count == 0 || !IsCpuRespawn) return;
		//TODO: check respawn Player cpu
		/*
		var newPlayer = new Player
		{
			Type = Type,
			Position = Players.First().Position
		};

		newPlayer._Process(FrameworkData.ProcessSpeed / Constants.BaseFramerate);
		*/
	}

	public override void _Process(double delta)
	{
		if (FrameworkData.IsPaused || !FrameworkData.UpdateObjects && !IsDead) return;
		
		float processSpeed = FrameworkData.ProcessSpeed;
		
		// Process local input
		_input.Update(Id);

		// Process Edit Mode
		if (_editMode. ProcessEditMode(processSpeed)) return;
	    
		// Process CPU Player logic (return if flying in or respawning)
		if (ProcessCpu(processSpeed)) return;
	    
		// Process Restart Event
		ProcessRestart(processSpeed);
	    
		// Process default player control routine
		UpdatePhysics();

		Camera.Main?.UpdatePlayerCamera(this);
		UpdateStatus();
		ProcessWater();
		RecordData();
		ProcessRotation();
		Sprite.Animate(new PlayerAnimationData(Type, Facing, IsSuper, GroundSpeed, Speed, ActionValue, CarryTarget));
		UpdateTail();
		ProcessPalette();
		UpdateCollision();
	}

	private void UpdateTail()
	{
		if (Tail == null) return;
		if (Type != Types.Tails)
		{
			Tail.QueueFree();
			return;
		}
			
		Tail.Animate(new TailAnimationData(Sprite.AnimationType, Sprite.Scale, Speed, GroundSpeed, 
			IsGrounded, IsSpinning, Angle, VisualAngle));
	}

	#region UpdatePlayerSystems
	
	private void UpdateStatus()
	{
		if (IsDead) return;

		// TODO: find a better place for this (and make obj_dust_skid)
		if (Sprite.AnimationType == Animations.Skid && Sprite.AnimationTimer % 4 == 0)
		{
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
	
		if (InvincibilityFrames > 0f)
		{
			Visible = ((int)InvincibilityFrames-- & 4) >= 1 || InvincibilityFrames == 0f;
		}
	
		if (ItemSpeedTimer > 0f && --ItemSpeedTimer == 0f)
		{
			//TODO: audio
			//stage_reset_bgm();	
		}
	
		if (ItemInvincibilityTimer > 0f && --ItemInvincibilityTimer == 0f)
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
		
			if (SuperValue == 0f)
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
					SuperValue = 60f;
				}
			}
			else
			{
				SuperValue--;
			}
		}
	
		IsInvincible = InvincibilityFrames != 0 || ItemInvincibilityTimer != 0 || 
		               IsHurt || IsSuper || Barrier.State == Barrier.States.DoubleSpin;
				 
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

					ZIndex = 0;
					Sprite.AnimationType = Animations.Drown;
					TileLayer = Constants.TileLayers.None;
					Speed = Vector2.Zero;
					Gravity	= 0.0625f;
					IsAirLock = true;
					if (Camera.Main == null) return;
					Camera.Main.Target = null;
					return;
				
				case -1f:
					if (Camera.Main == null) return;
					if ((int)Position.Y <= Camera.Main.BufferPosition.Y + SharedData.GameHeight + 276) return;
					
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
		
		SetSolid(new Vector2I(RadiusNormal.X + 1, Radius.Y));
		SetRegularHitbox();
		SetExtraHitbox();
	}

	private void SetRegularHitbox()
	{
		if (Sprite.AnimationType != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitbox(new Vector2I(8, Radius.Y - 3));
			return;
		}

		if (Type is Types.Tails or Types.Amy) return;
		SetHitbox(new Vector2I(8, 10), new Vector2I(0, 6));
	}

	private void SetExtraHitbox()
	{
		switch (Sprite.AnimationType)
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
		
		RecordedData.Add(new RecordedData(Position, _input.Press, _input.Down, PushingObject, Facing));
		if (RecordedData.Count <= CpuDelay * 2) return;
		RecordedData.RemoveAt(0);
	}

	private void ProcessRotation()
	{
		if (Sprite.AnimationType != Animations.Move)
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

	#endregion
	
	public void ResetGravity()
	{
		Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	}

	public void IncreaseComboScore(int comboCounter = 0)
	{
		ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
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

	private bool ProcessCpu(float processSpeed)
	{
		if (IsHurt || Id == 0) return false;
		
		// Find a player to follow
		CpuTarget ??= Players[Id - 1];

		if (RecordedData.Count < CpuDelay) return false;
		
		// Read actual player input and disable AI for 10 seconds if detected it
		if (InputDown.Abc || InputDown.Up || InputDown.Down || InputDown.Left || InputDown.Right)
		{
			CpuInputTimer = 600f;
		}

		return CpuState switch
		{
			CpuStates.RespawnInit => InitRespawnCpu(),
			CpuStates.Respawn => ProcessRespawnCpu(RecordedData[^CpuDelay]),
			CpuStates.Main => ProcessMainCpu(RecordedData[^CpuDelay]),
			CpuStates.Stuck => ProcessStuckCpu(),
			_ => false
		};
	}

	private bool InitRespawnCpu()
	{
		// ???
		if (InputDown is { Abc: false, Start: false })
		{
			if (!FrameworkData.IsTimePeriodLooped(64f) || !CpuTarget.ObjectInteraction) return false;
		}
				
		//TODO: ZIndex;
		//depth = 75;
		Position = (Vector2I)CpuTarget.Position - new Vector2I(16, 192);
				
		ObjectInteraction = false;
		CpuState = CpuStates.Respawn;
		return false;
	}

	private bool ProcessRespawnCpu(RecordedData followData)
	{
		if (!RespawnCpu())
		{
			if (Type == Types.Tails)
			{
				Sprite.AnimationType = Animations.Fly;
			}
					
			OnObject = null;
			IsGrounded = false;
					
			// Run animation script since we exit the entire player object code later
			Sprite.Animate(new PlayerAnimationData(
				Type, Facing, IsSuper, GroundSpeed, Speed, ActionValue, CarryTarget));
		}
				
		float distanceX = Position.X - followData.Position.X;
		if (distanceX != 0f)
		{
			float velocityX = 1f + Math.Abs(CpuTarget.Speed.X) + Math.Min(MathF.Floor(Math.Abs(distanceX) / 16), 12);
					
			if (distanceX >= 0f)
			{
				Facing = Constants.Direction.Negative;
						
				if (velocityX >= distanceX)
				{
					velocityX = -distanceX;
					distanceX = 0f;
				}
				else
				{
					velocityX = -velocityX;
				}
			}
			else
			{
				Facing = Constants.Direction.Positive;
				distanceX = -distanceX;
						
				if (velocityX >= distanceX)
				{
					velocityX  = -distanceX;
					distanceX = 0;
				}
			}
					
			Position += new Vector2(velocityX, 0f);
		}
				
		float distanceY = Mathf.FloorToInt(followData.Position.Y - Position.Y);
		if (distanceY != 0)
		{
			Position += new Vector2(0f, Math.Sign(distanceY));
		}
				
		if (!CpuTarget.IsDead && followData.Position.Y >= 0 && distanceX == 0 && distanceY == 0)
		{
			CpuState = CpuStates.Main;
			Sprite.AnimationType = Animations.Move;
			Speed = Vector2.Zero;
			GroundSpeed = 0f;
			GroundLockTimer = 0f;
			ObjectInteraction = true;
			ZIndex = RespawnData.ZIndex;
			ResetGravity();
			ResetState();
		}
		else
		{
			_input.Clear();
		}
				
		// Exit the entire player object code
		return true;
	}

	private bool ProcessMainCpu(RecordedData followData)
	{
		if (RespawnCpu()) return true;
		//TODO: check if behind player and main player tail
		ZIndex = CpuTarget.ZIndex;
		
		if (CpuInputTimer > 0f)
		{
			CpuInputTimer--;
			return false;
		}
		
		if (CarryTarget != null || Action == Actions.Carried) return false;

		if (GroundLockTimer != 0f && GroundSpeed == 0f)
		{
			CpuState = CpuStates.Stuck;
		}
		
		if (CpuTarget.Action == Actions.PeelOut)
		{
			followData.InputDown = new Buttons();
			followData.InputPress = new Buttons();
		}
		
		if (SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			if (Math.Abs(CpuTarget.GroundSpeed) < 4f && CpuTarget.OnObject == null)
			{
				followData.Position.X -= 32f;
			}
		}
	
		var doJump = true;
			
		// TODO: AI is pushing weirdly rn
		if (PushingObject == null || followData.PushingObject != null)
		{
			int distanceX = Mathf.FloorToInt(followData.Position.X - Position.X);
			if (distanceX != 0)
			{
				int maxDistanceX = SharedData.PlayerPhysics > PhysicsTypes.S3 ? 48 : 16;
				
				if (distanceX > 0)
				{
					if (distanceX > maxDistanceX)
					{
						followData.InputDown.Left = false;
						followData.InputPress.Left = false;
						followData.InputDown.Right = true;
						followData.InputPress.Right = true;
					}
						
					if (GroundSpeed != 0f && Facing == Constants.Direction.Positive)
					{
						Position += Vector2.Right;
					}
				}
				else
				{
					if (distanceX < -maxDistanceX)
					{
						followData.InputDown.Left = true;
						followData.InputPress.Left = true;
						followData.InputDown.Right = false;
						followData.InputPress.Right = false;
					}
					
					if (GroundSpeed != 0f && Facing == Constants.Direction.Negative)
					{
						Position += Vector2.Left;
					}
				}
			}
			else
			{
				Facing = followData.Facing;
			}
				
			if (!IsCpuJumping)
			{
				if (Math.Abs(distanceX) > 64 && !FrameworkData.IsTimePeriodLooped(256f))
				{
					doJump = false;
				}
				else
				{
					if (Mathf.FloorToInt(followData.Position.Y - Position.Y) > -32)
					{
						doJump = false;
					}
				}
			}
			else
			{
				followData.InputDown.Abc = true;
				
				if (IsGrounded)
				{
					IsCpuJumping = false;
				}
				else
				{
					doJump = false;
				}
			}
		}
		
		if (doJump && Sprite.AnimationType != Animations.Duck && FrameworkData.IsTimePeriodLooped(64f))
		{
			followData.InputPress.Abc = true;
			followData.InputDown.Abc = true;
			IsCpuJumping = true;
		}
		
		_input.Set(followData.InputPress, followData.InputDown);
		return false;
	}

	private bool ProcessStuckCpu()
	{
		if (RespawnCpu()) return true;
				
		if (GroundLockTimer != 0f || CpuInputTimer != 0f || GroundSpeed != 0f) return false;
				
		if (Sprite.AnimationType == Animations.Idle)
		{
			Facing = MathF.Floor(CpuTarget.Position.X - Position.X) > 0f ? 
				Constants.Direction.Positive : Constants.Direction.Negative;
		}
				
		_input.Down.Down = true;
				
		if (!FrameworkData.IsTimePeriodLooped(128f))
		{
			if (FrameworkData.IsTimePeriodLooped(32f))
			{
				_input.Press.Abc = true;
			}

			return false;
		}

		_input.Down.Down = false;
		_input.Press.Abc = false;
		CpuState = CpuStates.Main;
		
		return false;
	}

	private bool RespawnCpu()
	{
		if (Sprite != null && Sprite.CheckInView())
		{
			CpuTimer = 0f;
			return false;
		}

		if (++CpuTimer < 300f) return false;
		Init();
		return true;
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

				if (Camera.Main != null)
				{
					if (FrameworkData.PlayerPhysics < PhysicsTypes.S3)
					{
						bound += Camera.Main.LimitBottom * processSpeed; // TODO: check if LimitBottom or Bounds
					}
					else
					{
						bound += Camera.Main.BufferPosition.Y * processSpeed + SharedData.GameHeight;
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

	public void Kill()
	{
		if (IsDead) return;

		Action = Actions.None;
		IsDead = true;
		ObjectInteraction = false;
		IsGrounded = false;
		OnObject = null;
		Barrier.Type = Barrier.Types.None;
		Sprite.AnimationType = Animations.Death;
		Gravity = GravityType.Default;
		Speed = new Vector2(0f, -7f);
		GroundSpeed = 0f;
		ZIndex = 0;
		
		if (Id == 0)
		{
			FrameworkData.UpdateObjects = false;
			FrameworkData.UpdateTimer = false;
			FrameworkData.AllowPause = false;
		}
		
		//TODO: Audio
		//audio_play_sfx(sfx_hurt);
	}
	
	private void ReleaseDropDash()
	{
		if (!SharedData.DropDash || Action != Actions.DropDash) return;

		if (ActionValue < MaxDropDashCharge) return;
		
		Position += new Vector2(0f, Radius.Y - RadiusSpin.Y);
		Radius = RadiusSpin;

		if (IsSuper)
		{
			UpdateDropDashGroundSpeed(13f, 12f);
			Camera.Main?.SetShakeTimer(6);
		}
		else
		{
			UpdateDropDashGroundSpeed(12f, 8f);
		}
		
		Sprite.AnimationType = Animations.Spin;
		IsSpinning = true;
		
		if (!SharedData.CDCamera && Camera.Main != null)
		{
			Camera.Main.Delay.X = 8;
		}
			
		//TODO: audio & obj_dust_dropdash
		//instance_create(x, y + Radius.Y, obj_dust_dropdash, { image_xscale: Facing });
		//audio_stop_sfx(sfx_charge);
		//audio_play_sfx(sfx_release);
	}

	private void SetDropDashGroundSpeed(float force, float maxSpeed, Constants.Direction facing)
	{
		var sign = (float)facing;
		force *= sign;
		maxSpeed *= sign;
		
		if (sign * Speed.X >= 0)
		{
			GroundSpeed = MathF.Floor(GroundSpeed / 4f) + force;
			if (sign * GroundSpeed <= maxSpeed) return;
			GroundSpeed = maxSpeed;
			return;
		}
		
		GroundSpeed = (Mathf.IsEqualApprox(Angle, 360f) ? 0f : MathF.Floor(GroundSpeed / 2f)) + force;
	}
	
	public void ClearPush()
	{
		if (PushingObject != this) return;
		if (Sprite.AnimationType != Animations.Spin)
		{
			Sprite.AnimationType = Animations.Move;
		}
		
		PushingObject = null;
	}
}
