using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct PhysicsCore(PlayerData data)
{
	public enum Types : byte
	{
		S1, CD, S2, S3, SK
	}
	
	private Rolling _rolling = new(data);
	private Movement _movement = new(data);
	private Position _position = new(data);
	private Balancing _balancing = new(data);
	private Collision _collision = new(data);
	private SlopeRepel _slopeRepel = new(data);
	private SlopeResist _slopeResist = new(data);
	private CameraBounds _cameraBounds = new(data);

	public void ProcessCorePhysics()
	{
		if (data.Movement.IsCorePhysicsSkipped)
		{
			data.Movement.IsCorePhysicsSkipped = false;
			return;
		}
		
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
