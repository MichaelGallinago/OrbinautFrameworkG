using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Data;

public class PlayerData
{
	private const byte MinimalRecordLength = 32;

	public IPlayer Owner { get; set; }
	public int Id { get; set; }
	public Animations Animation { get; set; }
	public bool IsAnimationFrameChanged { get; set; }
	public int? OverrideAnimationFrame { get; set; }

	public SuperData SuperData { get; set; } = new();

	public Constants.TileLayers TileLayer { get; set; }
	public Constants.TileBehaviours TileBehaviour { get; set; }
	public bool IsStickToConvex { get; set; }
    
	public bool IsObjectInteractionEnabled { get; set; }
	public bool IsControlRoutineEnabled { get; set; }
	public bool IsGrounded { get; set; } = true;
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public OrbinautData SetPushAnimationBy { get; set; }
	public bool IsHurt { get; set; }
	public OrbinautData OnObject { get; set; }
	public bool IsInvincible { get; set; }

	public Actions Action;
    
	public Constants.Direction Facing { get; set; }
	
	public bool IsForcedSpin { get; set; }
	public float GroundLockTimer { get; set; }
	public bool IsAirLock { get; set; }
	
	public uint ComboCounter { get; set; }
	public float InvincibilityTimer { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }
	
	public float RestartTimer { get; set; }
	public PlayerInput Input { get; } = new();
	
	public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
	private DataRecord[] _recordedData;
	public TileCollider TileCollider = new();

	public PlayerData(IPlayer player)
	{
		Action = new Actions(this);
		if (ApplyType(player)) return;
		Init();
	}
	
	public void Record()
	{
		Array.Copy(_recordedData, 0, _recordedData, 
			1, _recordedData.Length - 1);
		
		_recordedData[0] = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
	}
	
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
        TileLayer = Constants.TileLayers.Main;
        TileBehaviour = Constants.TileBehaviours.Floor;
        IsStickToConvex = false;
        
        OnObject = null;
        IsGrounded = true;
        IsSpinning = false;
        IsJumping = false;
        IsHurt = false;
        IsInvincible = false;
        IsForcedSpin = false;
        
        GroundLockTimer = 0f;
        SuperData.Timer = 0f;

        IsObjectInteractionEnabled = true;
        IsControlRoutineEnabled = true;

        Action.Type = Actions.Types.Default;
        
        Facing = Constants.Direction.Positive;
        Animation = Animations.Idle;
        SetPushAnimationBy = null;
        ComboCounter = 0;
        
        InvincibilityTimer = 0f;
        ItemSpeedTimer = 0f;
        ItemInvincibilityTimer = 0f;
        
        RestartTimer = 0f;
        
        Input.Clear();
        Input.NoControl = false;
        
        (_collisionBoxes.RadiusNormal, _collisionBoxes.RadiusSpin) = Type switch
        {
	        Types.Tails => (new Vector2I(9, 15), new Vector2I(7, 14)),
	        Types.Amy => (new Vector2I(9, 16), new Vector2I(7, 12)),
	        _ => (new Vector2I(9, 19), new Vector2I(7, 14))
        };
        _collisionBoxes.Radius = _collisionBoxes.RadiusNormal;
		
        _physicsCore.Gravity = GravityType.Default;
        _physicsCore.Velocity.Vector = Vector2.Zero;
        _physicsCore.GroundSpeed.Value = 0f;
        
        _angleRotation.Angle = 0f;
        _angleRotation.VisualAngle = 0f;
		
        _water.IsUnderwater = false;
        _water.AirTimer = Constants.DefaultAirTimer;
        
        _death.IsDead = false;
        _death.State = Death.States.Wait;
		
        Shield.State = ShieldContainer.States.None;
        
        _carry.Target = null;
        _carry.Timer = 0f;
        _carry.TargetPosition = Vector2.Zero;

        if (_cpuModule != null)
        {
	        _cpuModule.Target = null;
	        _cpuModule.State = CpuModule.States.Main;
	        _cpuModule.RespawnTimer = 0f;
	        _cpuModule.InputTimer = 0f;
	        _cpuModule.IsJumping = false;
        }

        FillRecordedData();
        
        Owner.RotationDegrees = 0f;
        Owner.Visible = true;
	}

	public void FillRecordedData()
	{
		_recordedData = new DataRecord[Math.Max(MinimalRecordLength, CpuModule.DelayStep * Scene.Instance.Players.Count)];
		var record = new DataRecord(Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
        
		Array.Fill(_recordedData, record);
	}
	
	public static void ResizeAllRecordedData()
	{
		if (Scene.Instance.Time == 0f) return;

		ReadOnlySpan<Player> players = Scene.Instance.Players.Values;
		int playersCount = players.Length + 1;
		foreach (Player player in players)
		{
			player.Data.ResizeRecordedData(playersCount);
		}
	}

	private void ResizeRecordedData(int playersCount)
	{
		int newLength = Math.Max(MinimalRecordLength, CpuModule.DelayStep * playersCount);
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
	
	private bool ApplyType(Player player)
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
		player.QueueFree();
		return true;
	}
}
