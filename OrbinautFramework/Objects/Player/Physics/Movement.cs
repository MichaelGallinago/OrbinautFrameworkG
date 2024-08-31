using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Physics.Movements;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Movement(PlayerData data, IPlayerLogic logic)
{
	public readonly Ground Ground = new(data, logic);
	public readonly Rolling Rolling = new(data);
	public readonly Air Air = new(data, logic);
}
