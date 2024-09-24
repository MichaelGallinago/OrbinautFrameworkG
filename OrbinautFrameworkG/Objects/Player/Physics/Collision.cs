using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Physics.Collisions;

namespace OrbinautFrameworkG.Objects.Player.Physics;

public readonly struct Collision(PlayerData data, IPlayerLogic logic)
{
	public readonly Air Air = new(data, logic);
	public readonly Ground Ground = new(data, logic);
}