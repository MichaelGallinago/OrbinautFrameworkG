using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public class PlayerData
{
	private const byte MinimalRecordLength = 32;
	
	public event Action<Types> TypeChanged;
	
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
	
	public PhysicParams PhysicParams;
	public Velocity Velocity { get; } = new();
	public AcceleratedValue GroundSpeed { get; set; } = new();
	
	public int Id { get; set; }
	public Animations Animation { get; set; }
	public bool IsAnimationFrameChanged { get; set; }
	public int? OverrideAnimationFrame { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public float Angle { get; set; }

	public SuperData SuperData { get; set; } = new();

	public Constants.TileLayers TileLayer { get; set; }
	public Constants.TileBehaviours TileBehaviour { get; set; }
	public bool IsStickToConvex { get; set; }
    
	public bool IsObjectInteractionEnabled { get; set; }
	public bool IsControlRoutineEnabled { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public OrbinautData SetPushAnimationBy { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public DeathStates DeathState { get; set; }
	public OrbinautData OnObject { get; set; }
	public bool IsInvincible { get; set; }

	public Actions Action;
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

	public CarryData CarryData { get; set; } = new();

	public CpuData CpuData { get; set; } = new();
	
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }
	public PlayerInput Input { get; } = new();
	
	public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
	private DataRecord[] _recordedData;
	protected TileCollider TileCollider = new();

	public PlayerData(Player player)
	{
		Action = new Actions(player);
	}
	
	public void Record()
	{
		Array.Copy(_recordedData, 0, _recordedData, 
			1, _recordedData.Length - 1);
		
		_recordedData[0] = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
	}
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	
	public void ResetState()
	{
		Action.Type = Actions.Types.Default;
		
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsGrounded = false;
		
		OnObject = null;
		SetPushAnimationBy = null;
		
		Radius = RadiusNormal;
	}
	
	public void ResetMusic()
	{
		if (SuperData.IsSuper)
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
	
	public void Init()
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
		SuperData.Timer = 0f;

		IsObjectInteractionEnabled = true;
		IsControlRoutineEnabled = true;

		Action.Type = Actions.Types.Default;
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
		CpuRespawnTimer = 0f;
		CpuInputTimer = 0f;
		IsCpuJumping = false;
		
		Input.Clear();
		Input.NoControl = false;

		Rotation = 0f;
		Visible = true;
		
		_recordedData = new DataRecord[Math.Max(MinimalRecordLength, CpuDelayStep * Scene.Local.Players.Count)];
		var record = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
		
		Array.Fill(_recordedData, record);

		Sprite.Animate(this);
	}
	
	public static void ResizeAllRecordedData()
	{
		if (Scene.Local.Time == 0f) return;

		ReadOnlySpan<Player> players = Scene.Local.Players.Values;
		int playersCount = players.Length + 1;
		foreach (Player player in players)
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
	
	public void AttachToPlayer(ICarrier carrier)
	{
		Facing = carrier.Facing;
		Velocity.Vector = carrier.Velocity.Vector;
		Position = carrier.Position + new Vector2(0f, 28f);
		Scale = new Vector2(Math.Abs(Scale.X) * (float)carrier.Facing, Scale.Y);
		
		carrier.CarryTargetPosition = Position;
	}

	public void Spawn()
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
