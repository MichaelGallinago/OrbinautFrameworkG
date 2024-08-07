using System;
using OrbinautFramework3.Framework;
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

	public PhysicsCore()
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
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;

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
