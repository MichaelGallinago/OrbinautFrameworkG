using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.Slopes;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct PhysicsCore(PlayerData data, IPlayerLogic logic)
{
	private readonly Rolling _rolling = new(data, logic);
	private readonly Movement _movement = new(data, logic);
	public readonly Position Position = new(data);
	private readonly Balancing _balancing = new(data, logic);
	public readonly Collision Collision = new(data, logic);
	private readonly SlopeRepel _slopeRepel = new(data);
	private readonly SlopeResist _slopeResist = new(data, logic);
	public readonly CameraBounds CameraBounds = new(data, logic);

	public void ProcessCorePhysics()
	{
		if (!data.Movement.IsGrounded)
		{
			_movement.Air.Move();
			CameraBounds.Match();

			if (logic.Action == ActionFsm.States.Carried) return;
			Position.UpdateAir();
			Collision.Air.Collide();
			return;
		}

		if (data.Movement.IsSpinning)
		{
			_slopeResist.ResistRoll();
			_movement.Rolling.Roll();
			Collision.Ground.CollideWalls();
		}
		else
		{
			_slopeResist.ResistWalk();
			_movement.Ground.Move();
			_balancing.Balance();
			Collision.Ground.CollideWalls();
			_rolling.Start();
		}
		
		CameraBounds.Match();
		Position.UpdateGround();
		Collision.Ground.CollideFloor();
		_slopeRepel.Apply();
	}
}
