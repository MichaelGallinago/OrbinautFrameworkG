using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject
{
	[Export] private Types _uniqueType;
	[Export] private SpawnTypes _spawnType;
	
	public static List<Player> Players { get; } = [];
	
	public int Id { get; protected set; }
	public Types Type { get; set; }
	
	public Animations Animation { get; set; }
	public int? OverrideAnimationFrame { get; set; }
	public PhysicParams PhysicParams { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public Vector2 Speed { get; set; }
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
	public BaseObject PushingObject { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public BaseObject OnObject { get; set; }
	public bool IsSuper { get; set; }
	public bool IsInvincible { get; set; }
	public float SuperValue { get; set; }

	public Actions Action { get; set; }
	public int ActionState { get; set; }
	public float ActionValue { get; set; }
	public float ActionValue2 { get; set; }
	public bool BarrierFlag { get; set; }
	public Barrier Barrier { get; set; } //= new Barrier(this);
    
	public Constants.Direction Facing { get; set; }
	public float VisualAngle { get; set; }
	
    
	public bool IsForcedRoll { get; set; }
	public float GroundLockTimer { get; set; }
	public bool IsAirLock { get; set; }
    
	public float AirTimer { get; set; }
	public uint ComboCounter { get; set; }
	public uint ScoreCount { get; set; }
	public uint RingCount { get; set; }
	public uint LifeCount { get; set; }
	public float InvincibilityTimer { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }
	public List<uint> LifeRewards { get; set; }

	public ICarried CarryTarget { get; set; }
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

	public List<RecordedData> RecordedData { get; set; } = [];
	
	public CollisionTileMap TileMap { get; set; }
	public CommonStage Stage { get; set; }
	public TileCollider TileCollider { get; set; } = new();

	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; protected set; }

	// Edit mode
	public bool IsEditMode { get; set; }
	
	protected readonly PlayerInput Input = new();
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	
	public virtual void ResetState()
	{
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsGrounded = false;
		StickToConvex = false;
		
		OnObject = null;
		PushingObject = null;
		
		Radius = RadiusNormal;
		Action = Actions.None;
		GroundMode = Constants.GroundMode.Floor;
	}

	public override void Init()
	{
		/*
		if (ApplyType()) return;

		(RadiusNormal, RadiusSpin) = Type switch
		{
			Types.Tails => (new Vector2I(9, 15), new Vector2I(7, 14)),
			Types.Amy => (new Vector2I(9, 16), new Vector2I(7, 12)),
			_ => (new Vector2I(9, 19), new Vector2I(7, 14))
		};

		Radius = RadiusNormal;
		Position = Position with { Y = Position.Y - Radius.Y - 1f };
		Gravity = GravityType.Default;
		Speed = Vector2.Zero;
		GroundSpeed = 0f;
		Angle = 0f;

		TileLayer = Constants.TileLayers.Main;
		collision_mode = 0;
		StickToConvex = false;
		
		PushingObject = null;
		OnObject = null;
		ObjectInteraction = true;
		IsGrounded = true;
		IsSpinning = false;
		IsJumping = false;
		IsUnderwater = false;
		IsHurt = false;
		IsDead = false;
		IsSuper = false;
		IsInvincible = false;
		
		Action = Actions.None;
		ActionState = 0;
		ActionValue = 0f;
		ActionValue2 = 0f;
		SuperValue = 0f;
		
		Barrier.State = Barrier.States.None;
		Barrier.Type = Barrier.Types.None;
		
		Facing = Constants.Direction.Positive;
		Animation = Animations.Idle;
		VisualAngle = 0f;
		
		camera_view_timer = 120;
		
		IsForcedRoll = false;
		IsAirLock = false;
		GroundLockTimer = 0f;
		
		AirTimer = Constants.AirValueMax;
		ComboCounter = 0;
		ScoreCount = 0;
		RingCount = 0;
		InvincibilityTimer = 0f;
		ItemSpeedTimer = 0f;
		ItemInvincibilityTimer = 0f;
		LifeCount = 0;
		LifeRewards = [];
		
		CarryTarget = null;
		CarryTimer = 0;
		CarryTargetPosition = Vector2.Zero;
		
		CpuTimer = 0f;
		CpuInputTimer = 0f;
		IsCpuJumping = false;
		
		if (!_is_respawned)
		{
			cpu_target = null;
			cpu_state = CPU_STATE_MAIN;
		}
		else
		{
			cpu_state = CPU_STATE_RESPAWN_INIT;
		}
		
		//TODO: restart state
		RestartState =;
		RestartTimer = 0f;
		
		Input.Clear();
		
		RecordedData.Clear();
		
		if (Id != 0)
		{
			Player leadPlayer = Players[Id - 1];
			
			if (_is_respawned)
			{
				Position = new Vector2(Camera.Main.BufferPosition.X + 127, leadPlayer.Position.Y - 192);
			}
			else
			{
				Position = leadPlayer.Position - new Vector2(16, Radius.Y - leadPlayer.Radius.Y);
			}
		}
		else if (!_is_respawned)
		{
			if array_length(global.giant_ring_data) > 0
			{
				x = global.giant_ring_data[0];
				y = global.giant_ring_data[1];
				
				global.giant_ring_data = [];
			}
			else if array_length(global.checkpoint_data) > 0
			{
				x = global.checkpoint_data[0];
				y = global.checkpoint_data[1] - radius_y - 1;
			}
			
			c_engine.camera.target = id;
			c_engine.camera.pos_x = x - global.game_width / 2;
			c_engine.camera.pos_y = y - global.game_height / 2 + 16;

			if global.saved_rings != 0
			{
				ring_count = global.saved_rings;
			}
		
			if global.saved_barrier != BARRIER_NONE
			{
				barrier_type = global.saved_barrier;
				
				instance_create(x, y, obj_barrier, { TargetPlayer: id });
			}
			
			score_count = global.saved_score;
			life_count = global.saved_lives;
			
			global.saved_rings = 0;
			global.saved_barrier = BARRIER_NONE;
		}
		
		if (Type == Types.Tails)
		{
			with obj_tail
			{
				if TargetPlayer == other.id
				{
					instance_destroy();
				}
			}
			
			instance_create(x, y, obj_tail, { TargetPlayer: id, depth: depth + 1 });
		}
		
		// Apply initial animation
		scr_player_animate();
		*/
	}
	
	private bool ApplyType()
	{
		Type = _spawnType switch
		{
			SpawnTypes.Global => SharedData.PlayerType,
			SpawnTypes.GlobalAI => SharedData.PlayerTypeCpu,
			_ => Type
		};

		if (Type != Types.None) return false;
		QueueFree();
		return true;
	}
}
