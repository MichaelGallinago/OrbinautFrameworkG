using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject
{
	[Export] public Types Type;
	[Export] public SpawnTypes SpawnType;
	[Export] public PlayerAnimatedSprite Sprite { get; private set; }
	[Export] public PackedScene PackedTail { get; private set; }
	
	public static List<Player> Players { get; } = [];
	
	public int Id { get; protected set; }
	
	public Animations Animation { get; set; }
	public PhysicParams PhysicParams { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public Vector2 Speed;
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
	public Barrier Barrier { get; set; }
    
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
	public float InvincibilityFrames { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }
	public uint[] LifeRewards { get; set; }

	public Player CarryTarget { get; set; }
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

	public List<RecordedData> RecordedData { get; set; }
	
	public CollisionTileMap TileMap { get; set; }
	public CommonStage Stage { get; set; }
	public TileCollider TileCollider { get; set; }

	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; protected set; }

	// Edit mode
	public bool IsEditMode { get; set; }
	
	public Tail Tail { get; set; }
}