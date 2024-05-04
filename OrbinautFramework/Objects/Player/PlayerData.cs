using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject, ICpuTarget, IAnimatedPlayer
{
	private const byte MinimalRecordLength = 32;
	protected const int CpuDelayStep = 16;
	
	[Export] public PlayerAnimatedSprite Sprite { get; private set; }
	[Export] public ShieldContainer Shield { get; set; }
	[Export] private SpawnTypes _spawnType;
	[Export] private Types _uniqueType;

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
	
	public int Id { get; protected set; } = Players.Count;
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
	public DeathStates DeathState { get; set; }
	public BaseObject OnObject { get; set; }
	public bool IsInvincible { get; set; }
	public float SuperTimer { get; set; }
	
	public Actions Action { get; set; }
	public int ActionState { get; set; }
	public float ActionValue { get; set; }
	public float ActionValue2 { get; set; }
    
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
	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; } = [];
	public HashSet<BaseObject> PushObjects { get; } = [];
	public bool IsDebugMode { get; set; }
	public PlayerInput Input { get; } = new();
	
	public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
	private DataRecord[] _recordedData;
	protected readonly TileCollider TileCollider = new();

	public override void _Ready()
	{
		base._Ready();
		Spawn();
		InitializeCamera();
		Sprite.FrameChanged += () => IsAnimationFrameChanged = true;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		ResizeAllRecordedData();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		ResizeAllRecordedData();
	}

	//TODO: remove or use this
	public static void ResetStatic()
	{
		Players.Clear();
	}
	
	protected void RecordData()
	{
		Array.Copy(_recordedData, 0, _recordedData, 
			1, _recordedData.Length - 1);
		
		_recordedData[0] = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
	}
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	
	public void ResetState()
	{
		Action = Actions.None;
		
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsGrounded = false;
		
		OnObject = null;
		SetPushAnimationBy = null;
		
		Radius = RadiusNormal;
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
		DeathState = DeathStates.Wait;
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
		var record = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
		
		Array.Fill(_recordedData, record);
		
		Sprite.Animate(this);
	}
	
	private static void ResizeAllRecordedData()
	{
		if (Scene.Local.Time == 0f) return;

		int playersCount = Players.Count + 1;
		foreach (Player player in Players)
		{
			player.ResizeRecordedData(playersCount);
		}
	}

	private void ResizeRecordedData(int playersCount)
	{
		int newLength = Math.Max(MinimalRecordLength, CpuDelayStep * playersCount);
		int oldLength = _recordedData.Length;
		
		if (newLength <= oldLength)
		{
			Array.Resize(ref _recordedData, newLength);
			return;
		}
		
		var resizedData = new DataRecord[newLength];
		var record = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
		
		Array.Copy(_recordedData, resizedData, oldLength);
		Array.Fill(resizedData, record,oldLength, newLength - oldLength);
		_recordedData = resizedData;
	}

	private void Spawn()
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
		
		if (Id == 0 && SharedData.PlayerShield != ShieldContainer.Types.None)
		{
			// TODO: create shield
			//instance_create(x, y, obj_shield, { TargetPlayer: id });
		}
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
