using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Physics.Collisions;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Collision(PlayerData data, IPlayerLogic logic)
{
	private Air _air = new(data, logic);
	private Ground _ground = new(data, logic);

	public void CollideAir() => _air.Collide();
	public void CollideFloorGround() => _ground.CollideFloor();
	public void CollideWallsGround() => _ground.CollideWalls();
}