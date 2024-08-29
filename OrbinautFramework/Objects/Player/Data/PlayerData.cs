using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Data;

public class PlayerData(IPlayerNode node, IPlayerSprite sprite) : IPlayerData //TODO: remove or edit interface?
{
	public int Id { get; set; }
	public IPlayerNode Node { get; } = node;
	public IPlayerSprite Sprite { get; } = sprite;
	public PlayerStates State { get; set; }
	
	public CpuData Cpu { get; } = new();
	public ItemData Item { get; } = new();
	public CarryData Carry { get; } = new();
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
