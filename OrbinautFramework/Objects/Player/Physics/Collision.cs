using OrbinautFramework3.Objects.Player.Physics.Collisions;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Collision
{
	private Air _air;
	private Ground _ground;

	public Collision()
	{
		_air = new Air();
		_ground = new Ground();
	}

	public void CollideAir() => _air.Collide();
	public void CollideFloorGround() => _ground.CollideFloor();
	public void CollideWallsGround() => _ground.CollideWalls();
}