using System;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct PhysicsCore
{
	public enum Types : byte
	{
		S1, CD, S2, S3, SK
	}

	private SlopeRepel _slopeRepel;
	private SlopeResist _slopeResist;
	private Movement _movement;
	private Balancing _balancing;
	private Collision _collision;
	private Rolling _rolling;
	private CameraBounds _cameraBounds;
	private Position _position;
	
	private PhysicParams _physicParams;

	public PhysicsCore(PlayerData data)
	{
		_slopeRepel = new SlopeRepel(data);
		_slopeResist = new SlopeResist(data);
		_movement = new Movement(data);
		_balancing = new Balancing(data);
		_collision = new Collision(data);
		_rolling = new Rolling(data);
		_cameraBounds = new CameraBounds(data);
		_position = new Position(data);
	}

	public void UpdatePhysicParameters()
	{
		_physicParams = PhysicParams.Get(IsUnderwater, IsSuper, Type, ItemSpeedTimer);
	}

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
