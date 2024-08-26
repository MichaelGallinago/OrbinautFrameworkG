using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct PhysicsCore(PlayerData data, IPlayerLogic logic)
{
	private readonly Rolling _rolling = new(data, logic);
	private readonly Movement _movement = new(data, logic);
	private readonly Position _position = new(data, logic);
	private readonly Balancing _balancing = new(data, logic);
	private readonly Collision _collision = new(data, logic);
	private readonly SlopeRepel _slopeRepel = new(data, logic);
	private readonly SlopeResist _slopeResist = new(data, logic);
	private readonly CameraBounds _cameraBounds = new(data, logic);

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
