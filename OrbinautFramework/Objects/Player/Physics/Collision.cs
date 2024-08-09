using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics.Collisions;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Collision(PlayerData data)
{
	private Air _air = new(data);
	private Ground _ground = new(data);

	public void CollideAir() => _air.Collide();
	public void CollideFloorGround() => _ground.CollideFloor();
	public void CollideWallsGround() => _ground.CollideWalls();
}