using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Physics.Movements;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Movement(PlayerData data, IPlayerLogic logic)
{
	private readonly Ground _ground = new(data, logic);
	private readonly Rolling _rolling = new(data);
	private readonly Air _air = new(data, logic);

	public void Move()
	{
		_ground.Move();
		_rolling.Roll();
		_air.Move();
	}
}
