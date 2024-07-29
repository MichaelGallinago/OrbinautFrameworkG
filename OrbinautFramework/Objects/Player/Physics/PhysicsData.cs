using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct PhysicsData
{
	private SlopeRepel _slopeRepel;
	private SlopeResist _slopeResist;
	private Movement _movement;
	private Balancing _balancing;
	private Collision _collision;
	private Rolling _rolling;
	private CameraBounds _cameraBounds;
	private Position _position;
	
	private PhysicParams _physicParams;
	
	private Landing _landing = new();

	public PhysicsData()
	{
		_slopeRepel = new SlopeRepel();
		_slopeResist = new SlopeResist();
		_movement = new Movement();
		_balancing = new Balancing(); 
		_collision = new Collision();
		_rolling = new Rolling();
		_cameraBounds = new CameraBounds();
		_position = new Position();
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
