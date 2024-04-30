using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject, ICpuTarget
{
	private const byte MinimalRecordLength = 32;
	protected const int CpuDelayStep = 16;
	
	[Export] private Types _uniqueType;
	[Export] private SpawnTypes _spawnType;

	public event Action<Types> TypeChanged;
	private event Action<int> PlayerCountChanged;

	public static List<Player> Players { get; } = [];
	private static int _playerCount;

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
	public Constants.TileBehaviours TileBehaviour { get; set; }
	public bool IsStickToConvex { get; set; }
    
	public bool IsObjectInteractionEnabled { get; set; }
	public bool IsControlRoutineEnabled { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public BaseObject SetPushAnimationBy { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public bool IsRestartOnDeath { get; set; }
	public BaseObject OnObject { get; set; }
	public bool IsInvincible { get; set; }
	public float SuperTimer { get; set; }
	
	public Actions Action { get; set; }
	public int ActionState { get; set; }
	public float ActionValue { get; set; }
	public float ActionValue2 { get; set; }
	public ShieldContainer Shield { get; set; }
    
	public Constants.Direction Facing { get; set; }
	public float VisualAngle { get; set; }
	
	public bool IsForcedSpin { get; set; }
	public float GroundLockTimer { get; set; }
	public bool IsAirLock { get; set; }
    
	public float AirTimer { get; set; }
	public uint ComboCounter { get; set; }
	public float InvincibilityTimer { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }

	public ICarried CarryTarget { get; set; }
	public float CarryTimer { get; set; }
	public Vector2 CarryTargetPosition { get; set; }
    
	public CpuStates CpuState { get; set; } = CpuStates.Main;
	public float CpuTimer { get; set; }
	public float CpuInputTimer { get; set; }
	public bool IsCpuJumping { get; set; }
	public bool IsCpuRespawn { get; set; }
	public ICpuTarget CpuTarget { get; set; }
    
	public ICamera Camera { get; set; }
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }
	public Stage Stage { get; set; }
	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; } = [];
	public HashSet<BaseObject> PushObjects { get; } = [];
	public bool IsDebugMode { get; set; }
	public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
	private DataRecord[] _recordedData;
	
	protected CollisionTileMap TileMap;
	protected readonly PlayerInput Input = new();
	protected readonly TileCollider TileCollider = new();
	
	protected PlayerData()
	{
		Id = _playerCount++;
		PlayerCountChanged?.Invoke(_playerCount);
		Shield = new ShieldContainer(this);
	}

	public override void _Ready()
	{
		PlayerCountChanged += 
		base._Ready();
		Spawn();
		InitializeCamera();
	}

	public static void ResetStatic()
	{
		_playerCount = 0;
		Players.Clear();
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
	
	protected override void Init()
	{
		if (ApplyType()) return;
		
		(RadiusNormal, RadiusSpin) = Type switch
		{
			Types.Tails => (new Vector2I(9, 15), new Vector2I(7, 14)),
			Types.Amy => (new Vector2I(9, 16), new Vector2I(7, 12)),
			_ => (new Vector2I(9, 19), new Vector2I(7, 14))
		};
		
		Radius = RadiusNormal;
		Gravity = GravityType.Default;
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		Angle = 0f;
		
		TileLayer = Constants.TileLayers.Main;
		TileBehaviour = Constants.TileBehaviours.Floor;
		IsStickToConvex = false;
		
		OnObject = null;
		IsGrounded = true;
		IsSpinning = false;
		IsJumping = false;
		IsUnderwater = false;
		IsHurt = false;
		IsDead = false;
		IsInvincible = false;
		IsForcedSpin = false;
		
		GroundLockTimer = 0f;
		SuperTimer = 0f;

		IsObjectInteractionEnabled = true;
		IsControlRoutineEnabled = true;

		Action = Actions.None;
		ActionState = 0;
		ActionValue = 0;
		ActionValue2 = 0;

		Shield.State = ShieldContainer.States.None;
		Facing = Constants.Direction.Positive;
		Animation = Animations.Idle;
		SetPushAnimationBy = null;
		VisualAngle = 0f;
		AirTimer = Constants.DefaultAirTimer;
		ComboCounter = 0;
		
		InvincibilityTimer = 0f;
		ItemSpeedTimer = 0f;
		ItemInvincibilityTimer = 0f;
		
		CarryTarget = null;
		CarryTimer = 0f;
		CarryTargetPosition = Vector2.Zero;
		IsRestartOnDeath = false;
		RestartTimer = 0f;
		CpuTarget = null;
		CpuState = CpuStates.Main;
		CpuTimer = 0f;
		CpuInputTimer = 0f;
		IsCpuJumping = false;
		
		Input.Clear();
		Input.NoControl = false;

		Rotation = 0f;
		Visible = true;
		
		_recordedData = new DataRecord[Math.Max(MinimalRecordLength, CpuDelayStep * Players.Count)];
		var record = new DataRecord(
			Position, Input.Press, Input.Down, IsGrounded, IsJumping, Action, Facing, SetPushAnimationBy);
		
		Array.Fill(_recordedData, record);
	}

	private void ResizeRecordedData(int count)
	{
		var resizedData = new DataRecord[Math.Max(MinimalRecordLength, CpuDelayStep * count)];
		var record = new DataRecord(
			Position, Input.Press, Input.Down, IsGrounded, IsJumping, Action, Facing, SetPushAnimationBy);
		
		Array.Fill(_recordedData, record);
		_recordedData = 
	}

	private void Spawn()
	{
		if (Id == 0)
		{
			SpawnFirstPlayer();
			return;
		}

		SpawnFollowingPlayer();
	}

	private void SpawnFirstPlayer()
	{
		if (SharedData.GiantRingData != null)
		{
			Position = SharedData.GiantRingData.Position;
		}
		else
		{
			if (SharedData.CheckpointData != null)
			{
				Position = SharedData.CheckpointData.Position;
			}
			Position -= new Vector2(0, Radius.Y + 1);
		}
		
		if (SharedData.PlayerShield != ShieldContainer.Types.None)
		{
			// TODO: create shield
			//instance_create(x, y, obj_shield, { TargetPlayer: id });
		}
	}

	private void SpawnFollowingPlayer()
	{
		Player previousPlayer = Players[Id - 1];
		Position = previousPlayer.Position - new Vector2(16f, Radius.Y - previousPlayer.Radius.Y);
	}

	private void InitializeCamera()
	{
		ReadOnlySpan<ICamera> cameras = Views.Local.Cameras;
		if (cameras.Length <= Id) return;
		cameras[Id].Target = this;
		cameras[Id].Position = Position - SharedData.ViewSize / 2 + Vector2.Down * 16;
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
