using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics.Movements;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Movement(PlayerData data)
{
	private Ground _ground = new(data);
	private Rolling _rolling = new(data);
	private Air _air = new(data);

	public void Move()
	{
		_ground.Move();
		_rolling.Roll();
		_air.Move();
	}
}
