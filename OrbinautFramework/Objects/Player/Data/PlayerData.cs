using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Data;

public class PlayerData(IStateHolder<ActionFsm.States> stateHolder, IPlayerNode playerNode) 
	: IPlayerCameraTarget, IPlayerData, IPlayerCpuTarget
{
	public int Id { get; set; }
	public IPlayerNode PlayerNode { get; } = playerNode;
	
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

	public ActionFsm.States State
	{
		get => stateHolder.State; 
		set => stateHolder.State = value;
	}
	
	public void ResetState()
	{
		State = ActionFsm.States.Default;
		
		Damage.IsHurt = false;
		
		Physics.IsJumping = false;
		Physics.IsSpinning = false;
		Physics.IsGrounded = false;
		
		Visual.SetPushBy = null;
		
		Collision.OnObject = null;
		Collision.Radius = Collision.RadiusNormal;
	}
}
