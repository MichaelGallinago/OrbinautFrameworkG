using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Physics.Movements;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Movement(PlayerData data, IPlayerLogic logic)
{
	private Ground _ground = new(data, logic);
	private Rolling _rolling = new(data);
	private Air _air = new(data, logic);

	public void Move()
	{
		_ground.Move();
		_rolling.Roll();
		_air.Move();
	}
}
