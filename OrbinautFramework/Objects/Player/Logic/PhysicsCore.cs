using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Logic;

public struct PhysicsCore(PlayerData data, IPlayerLogic logic)
{
	private Rolling _rolling = new(data, logic);
	private Movement _movement = new(data, logic);
	private Position _position = new(data, logic);
	private Balancing _balancing = new(data, logic);
	private Collision _collision = new(data, logic);
	private SlopeRepel _slopeRepel = new(data, logic);
	private SlopeResist _slopeResist = new(data, logic);
	private CameraBounds _cameraBounds = new(data, logic);

	public void ProcessCorePhysics()
	{
		_slopeResist.Apply();
		_movement.Move();
		_balancing.Balance();
		_collision.CollideWallsGround();
		_rolling.Start();
		_cameraBounds.Match();
		_position.Update();
		_collision.CollideFloorGround();
		_slopeRepel.Apply();
		_collision.CollideAir();
	}
}
