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
	public const byte MaxRecordLength = 32;
	
	[Export] private Types _uniqueType;
	[Export] private SpawnTypes _spawnType;

	public event Action<Types> TypeChanged;

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
	public Constants.TileLayerBehaviours TileLayerBehaviour { get; set; }
	public bool StickToConvex { get; set; }
    
	public bool ObjectInteraction { get; set; }
	public bool IsRunControlRoutine { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public BaseObject SetPushAnimationBy { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
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
    
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }
	public CommonStage Stage { get; set; }
	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; } = [];
	public HashSet<BaseObject> PushObjects { get; } = [];
	public bool IsEditMode { get; set; }
	public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
	private DataRecord[] _recordedData;
	
	protected CollisionTileMap TileMap;
	protected readonly PlayerInput Input = new();
	protected readonly TileCollider TileCollider = new();
	
	private bool _isInit = true;
	
	protected PlayerData()
	{
		Id = _playerCount++;
		Shield = new ShieldContainer(this);
	}

	public override void _Ready()
	{
		Reset();
		Initialize();
		
		TileCollider.SetData((Vector2I)Position, TileLayer, TileMap);
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
		IsRunControlRoutine = true;
		IsGrounded = true;
		IsSpinning = false;
		IsJumping = false;
		IsUnderwater = false;
		IsHurt = false;
		IsRestartOnDeath = false;
		IsInvincible = false;
		
		Action = Actions.None;
		ActionState = 0;
		ActionValue = 0f;
		ActionValue2 = 0f;
		SuperTimer = 0f;
		
		Shield.State = ShieldContainer.States.None;
		Shield.Type = ShieldContainer.Types.None;
		
		Facing = Constants.Direction.Positive;
		Animation = Animations.Idle;
		VisualAngle = 0f;
		
		IsForcedSpin = false;
		IsAirLock = false;
		GroundLockTimer = 0f;
		
		AirTimer = Constants.DefaultAirValue;
		ComboCounter = 0;
		InvincibilityTimer = 0f;
		ItemSpeedTimer = 0f;
		ItemInvincibilityTimer = 0f;
		
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
		
		_recordedData = new DataRecord[MaxRecordLength * Players.Count];
		var record = new DataRecord(
			Position, Input.Press, Input.Down, IsGrounded, IsJumping, Action, Facing, SetPushAnimationBy);
		
		Array.Fill(_recordedData, record);

		if (!_isInit) return;
		_isInit = false;
	}

	private void Initialize()
	{
		// Spawn on position and load saved data (if exists)
		if (Id == 0)
		{
			var _ring_data = global.giant_ring_data;
			var _checkpoint_data = global.checkpoint_data;
			
			if array_length(_ring_data) > 0
			{
				x = _ring_data[0];
				y = _ring_data[1];
			}
			else if array_length(global.checkpoint_data) > 0
			{
				x = _checkpoint_data[0];
				y = _checkpoint_data[1] - radius_y - 1;
			}
			else
			{
				y -= radius_y + 1;
			}
		
			if global.player_shield != SHIELD_NONE
			{
				instance_create(x, y, obj_shield, { TargetPlayer: id });
			}
		}
	
		// Spawn behind the previous player
		else
		{
			var _previous_player = player_get(player_index - 1);
			
			x = _previous_player.x - 16;
			y = _previous_player.y + _previous_player.radius_y - radius_y;
		}
		
		if (SharedData.GiantRingData != null)
		{
			Position = SharedData.GiantRingData.Position;
		}
		else if (SharedData.CheckpointData != null)
		{
			Position = SharedData.CheckpointData.Position - new Vector2I(0, Radius.Y + 1);
		}
		else
		{
			Position = Position with { Y = Position.Y - Radius.Y - 1 };
		}
		
		
		if (SharedData.PlayerShield != ShieldContainer.Types.None)
		{
			Shield.Type = SharedData.PlayerShield;
		}
		
		InitializeCamera();
	}

	private void InitializeCamera()
	{
		ReadOnlySpan<ICamera> cameras = FrameworkData.CurrentScene.Views.Cameras;
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
