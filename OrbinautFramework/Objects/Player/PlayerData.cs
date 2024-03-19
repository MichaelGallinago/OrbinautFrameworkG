using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject, ICpuTarget
{
	[Export] private Types _uniqueType;
	[Export] private SpawnTypes _spawnType;

	public event Action<Types> TypeChanged;
	
	public static List<Player> Players { get; } = [];

	public Types Type
	{
		get => _type;
		set
		{
			TypeChanged?.Invoke(value);
			_type = value;
		}
	}
	private Types _type;
	public Velocity Velocity { get; } = new();
	public AcceleratedValue GroundSpeed { get; set; } = new();
	
	public int Id { get; protected set; }
	public Animations Animation { get; set; }
	public bool IsAnimationFrameChanged { get; set; }
	public int? OverrideAnimationFrame { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public float Angle { get; set; }
	public float SlopeGravity { get; set; }

	public Constants.TileLayers TileLayer { get; set; }
	public Constants.TileLayerBehaviours TileLayerBehaviour { get; set; }
	public bool StickToConvex { get; set; }
    
	public bool ObjectInteraction { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public BaseObject SetPushAnimationBy { get; set; }
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
	public Barrier Barrier { get; set; }
    
	public Constants.Direction Facing { get; set; }
	public float VisualAngle { get; set; }
	
	public bool IsForcedSpin { get; set; }
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
    
	public CpuStates CpuState { get; set; } = CpuStates.Main;
	public float CpuTimer { get; set; }
	public float CpuInputTimer { get; set; }
	public bool IsCpuJumping { get; set; }
	public bool IsCpuRespawn { get; set; }
	public ICpuTarget CpuTarget { get; set; }
    
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }
	public CommonStage Stage { get; set; }
	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; } = [];
	public HashSet<BaseObject> PushObjects { get; } = [];
	public bool IsEditMode { get; set; }
	public List<RecordedData> RecordedData { get; init; } = [];
	
	protected CollisionTileMap TileMap;
	protected readonly PlayerInput Input = new();
	protected readonly TileCollider TileCollider = new();
	
	private bool _isInit = true;
	
	protected PlayerData()
	{
		Barrier = new Barrier(this);
	}

	public override void _Ready()
	{
		Reset();
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
		LifeRewards = [RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000];
	}
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	
	public virtual void ResetState()
	{
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsGrounded = false;
		
		OnObject = null;
		SetPushAnimationBy = null;
		
		Radius = RadiusNormal;
		Action = Actions.None;
	}
	
	public override void Reset()
	{
		if (ApplyType()) return;
		
		base.Reset();
		
		(RadiusNormal, RadiusSpin) = Type switch
		{
			Types.Tails => (new Vector2I(9, 15), new Vector2I(7, 14)),
			Types.Amy => (new Vector2I(9, 16), new Vector2I(7, 12)),
			_ => (new Vector2I(9, 19), new Vector2I(7, 14))
		};
		
		Radius = RadiusNormal;
		Position = Position with { Y = Position.Y - Radius.Y - 1f };
		Gravity = GravityType.Default;
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		Angle = 0f;
		
		TileLayer = Constants.TileLayers.Main;
		TileLayerBehaviour = Constants.TileLayerBehaviours.Floor;
		StickToConvex = false;
		
		SetPushAnimationBy = null;
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
		
		if (Camera.Main != null && Camera.Main.Target == this)
		{
			Camera.Main.Target = this;
		}
		
		IsForcedSpin = false;
		IsAirLock = false;
		GroundLockTimer = 0f;
		
		AirTimer = Constants.DefaultAirValue;
		ComboCounter = 0;
		ScoreCount = 0;
		RingCount = 0;
		InvincibilityTimer = 0f;
		ItemSpeedTimer = 0f;
		ItemInvincibilityTimer = 0f;
		LifeCount = 0;
		LifeRewards = [];
		
		CarryTarget = null;
		CarryTimer = 0f;
		CarryTargetPosition = Vector2.Zero;
		
		CpuTimer = 0f;
		CpuInputTimer = 0f;
		IsCpuJumping = false;
		
		CpuState = _isInit ? CpuStates.Main : CpuStates.RespawnInit;
		
		RestartState = RestartStates.GameOver;
		RestartTimer = 0f;
		
		Input.Clear();
		Input.NoControl = false;
		RecordedData.Clear();
		
		if (Id != 0)
		{
			Player leadPlayer = Players[Id - 1];
			
			if (_isInit)
			{
				Position = leadPlayer.Position - new Vector2(16, Radius.Y - leadPlayer.Radius.Y);
				_isInit = false;
			}
			else
			{
				Position = new Vector2(Camera.Main.BufferPosition.X + sbyte.MaxValue, leadPlayer.Position.Y - 192);
			}
			return;
		}

		if (!_isInit) return;
		_isInit = false;
		
		if (SharedData.GiantRingData != null)
		{
			Position = SharedData.GiantRingData.position;
			SharedData.GiantRingData = null;
		}
		else if (SharedData.CheckpointData != null)
		{
			Position = SharedData.CheckpointData.Position - new Vector2I(0, Radius.Y + 1);
		}

		Camera.Main.Target = this;
		Camera.Main.Position = Position - SharedData.ViewSize / 2 - new Vector2(0, 16);

		if (SharedData.SavedRings != 0)
		{
			RingCount = SharedData.SavedRings;
		}
		
		if (SharedData.SavedBarrier != Barrier.Types.None)
		{
			Barrier.Type = SharedData.SavedBarrier;
		}
			
		ScoreCount = SharedData.SavedScore;
		LifeCount = SharedData.SavedLives;

		SharedData.SavedRings = 0;
		SharedData.SavedBarrier = Barrier.Types.None;
	}
	
	private bool ApplyType()
	{
		Type = _spawnType switch
		{
			SpawnTypes.Global => SharedData.PlayerType,
			SpawnTypes.GlobalAI => SharedData.PlayerTypeCpu,
			SpawnTypes.Unique => _uniqueType,
			_ => Type
		};
		_spawnType = SpawnTypes.None;
		
		if (Type != Types.None) return false;
		QueueFree();
		return true;
	}
}
