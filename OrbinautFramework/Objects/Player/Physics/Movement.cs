using OrbinautFramework3.Objects.Player.Physics.Movements;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Movement
{
	private Ground _ground;
	private Rolling _rolling;
	private Air _air;

	public Movement()
	{
		_ground = new Ground();
		_rolling = new Rolling();
		_air = new Air();
	}

	public void Move()
	{
		_ground.Move();
		_rolling.Roll();
		_air.Move();
	}
}
