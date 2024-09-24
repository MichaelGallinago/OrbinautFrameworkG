using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Physics.Movements;

namespace OrbinautFrameworkG.Objects.Player.Physics;

public readonly struct Movement(PlayerData data, IPlayerLogic logic)
{
	public readonly Ground Ground = new(data, logic);
	public readonly Rolling Rolling = new(data);
	public readonly Air Air = new(data, logic);
}
