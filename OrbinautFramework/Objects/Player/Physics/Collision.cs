using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Physics.Collisions;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Collision(PlayerData data, IPlayerLogic logic)
{
	public readonly Air Air = new(data, logic);
	public readonly Ground Ground = new(data, logic);
}