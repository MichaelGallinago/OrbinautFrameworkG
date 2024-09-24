using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class PlayerData(IPlayerNode node, IPlayerSprite sprite)
{
	public int Id { get; set; }
	public PlayerStates State { get; set; }
	
	public IPlayerNode Node { get; } = node;
	public IPlayerSprite Sprite { get; } = sprite;
	
	public CpuData Cpu { get; } = new();
	public ItemData Item { get; } = new();
	public DeathData Death { get; } = new();
	public InputData Input { get; } = new();
	public SuperData Super { get; } = new();
	public WaterData Water { get; } = new();
	public DamageData Damage { get; } = new();
	public VisualData Visual { get; } = new();
	public PhysicsData Physics { get; } = new();
	public MovementData Movement { get; } = new();
	public CollisionData Collision { get; } = new();
}
