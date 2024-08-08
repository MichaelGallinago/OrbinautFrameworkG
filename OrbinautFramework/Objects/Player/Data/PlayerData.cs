using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Data;

public class PlayerData : IPlayerCameraTarget, IActor, ICpuTarget
{
	public IPlayerNode PlayerNode { get; }
	public int Id { get; set; }
	
	public Actions.Types ActionType
	{
		get => Action.Type;
		set => Action.Type = value;
	}
	public Actions Action;
	
	public ItemData Item { get; } = new();
	public CarryData Carry { get; } = new();
	public DeathData Death { get; } = new();
	public SuperData Super { get; } = new();
	public WaterData Water { get; } = new();
	public DamageData Damage { get; } = new();
	public VisualData Visual { get; } = new();
	public PlayerInput Input { get; } = new();
	public PhysicsData Physics { get; } = new();
	public RotationData Rotation { get; } = new();
	public CollisionData Collision { get; } = new();
	public TileCollider TileCollider { get; } = new();
	
	public PlayerData(IPlayerNode playerNode)
	{
		PlayerNode = playerNode;
		Action = new Actions(this);
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
}
