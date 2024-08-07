using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Data;

public class PlayerData
{
	public IPlayer Owner { get; set; }
	public int Id { get; set; }

	public Actions Action;
	
	public SuperData Super { get; } = new();
	public PlayerInput Input { get; } = new();
	public TileCollider TileCollider { get; } = new();
	public CollisionData Collision { get; } = new();
	public PhysicsData Physics { get; } = new();
	public WaterData Water { get; } = new();
	public DeathData Death { get; } = new();
	public RotationData Rotation { get; } = new();
	public CarryData Carry { get; } = new();
	public DamageData Damage { get; } = new();
	public VisualData Visual { get; } = new();
	public ItemData Item { get; } = new();
	
	public PlayerData(IPlayer player)
	{
		Owner = player;
		Action = new Actions(this);
		Spawn();
		Init();
	}
	
	public void ResetState()
	{
		Action.Type = Actions.Types.Default;
		
		Damage.IsHurt = false;
		
		Physics.IsJumping = false;
		Physics.IsSpinning = false;
		Physics.IsGrounded = false;
		
		Visual.SetPushBy = null;
		
		Collision.OnObject = null;
		Collision.Radius = Collision.RadiusNormal;
	}
	
	public void Init()
	{
		Collision.Init(Owner.Type);
        Physics.Init();
        Super.Init();
        Visual.Init();
        Item.Init();
        Damage.Init();
        
        Action.Type = Actions.Types.Default;
        
        Input.Clear();
        Input.NoControl = false;
        
        Rotation.Angle = 0f;
        Rotation.VisualAngle = 0f;
		
        Water.IsUnderwater = false;
        Water.AirTimer = Constants.DefaultAirTimer;
        
        Death.RestartTimer = 0f;
        Death.IsDead = false;
        Death.State = Modules.Death.States.Wait;
		
        Shield.State = ShieldContainer.States.None;
        
        Carry.Timer = 0f;
        Carry.Target = null;
        Carry.TargetPosition = Vector2.Zero;
        
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
	
	public void Spawn()
	{
		if (SharedData.GiantRingData != null)
		{
			Owner.Position = SharedData.GiantRingData.Position;
		}
		else
		{
			if (SharedData.CheckpointData != null)
			{
				Owner.Position = SharedData.CheckpointData.Position;
			}
			Owner.Position -= new Vector2(0, Collision.Radius.Y + 1);
		}
		
		if (Id == 0 && SharedData.PlayerShield != ShieldContainer.Types.None)
		{
			// TODO: create shield
			//instance_create(x, y, obj_shield, { TargetPlayer: id });
		}
	}
}
