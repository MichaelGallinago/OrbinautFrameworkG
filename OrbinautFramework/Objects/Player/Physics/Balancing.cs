using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Balancing(PlayerData data)
{
    public void Balance()
	{
		if (!data.Movement.IsGrounded || data.Movement.IsSpinning) return;
		if (data.Movement.GroundSpeed != 0f || data.State is States.SpinDash or States.Dash) return;
		
		// Don't allow player to duck or look up
		if (SharedData.PhysicsType == PhysicsCore.Types.SK && (data.Input.Down.Down || data.Input.Down.Up)) return;
		
		if (BalanceOnTiles()) return;
		BalanceOnObject();
	}
	
	private void BalanceOnObject()
	{
		OrbinautNode onObject = data.Collision.OnObject;
		if (onObject == null) return;
		// TODO: check IsInstanceValid == instance_exist
		if (!GodotObject.IsInstanceValid(onObject) || onObject.SolidBox.NoBalance) return;
		
		const int leftEdge = 2;
		const int panicOffset = 4;
		
		int rightEdge = onObject.SolidBox.Radius.X * 2 - leftEdge;
		int playerX = Mathf.FloorToInt(onObject.SolidBox.Radius.X - onObject.Position.X + data.PlayerNode.Position.X);
		
		if (playerX < leftEdge)
		{
			BalanceToDirection(Constants.Direction.Negative, playerX < leftEdge - panicOffset);
		}
		else if (playerX > rightEdge)
		{
			BalanceToDirection(Constants.Direction.Positive, playerX > rightEdge + panicOffset);
		}
	}
	
	private bool BalanceOnTiles()
	{
		if (data.Collision.OnObject != null) return false;
		
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(data.Movement.Angle) > Angles.Quadrant.Down) return true;
		data.TileCollider.SetData(
			(Vector2I)data.PlayerNode.Position + new Vector2I(0, data.Collision.Radius.Y), 
			data.Collision.TileLayer);
		
		if (data.TileCollider.FindDistance(0, 0, true, direction) < 12) return true;
		
		(_, float angleLeft) = data.TileCollider.FindTile(-data.Collision.Radius.X, 0, true, direction);
		(_, float angleRight) = data.TileCollider.FindTile(data.Collision.Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) == float.IsNaN(angleRight)) return true;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = data.TileCollider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
		return true;
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (data.PlayerNode.Type)
		{
			case PlayerNode.Types.Amy or PlayerNode.Types.Tails:
			case PlayerNode.Types.Sonic when data.Super.IsSuper:
				data.Visual.Animation = Animations.Balance;
				data.Visual.Facing = direction;
				break;
			
			case PlayerNode.Types.Knuckles:
				if (data.Visual.Facing == direction)
				{
					data.Visual.Animation = Animations.Balance;
				}
				else if (data.Visual.Animation != Animations.BalanceFlip)
				{
					data.Visual.Animation = Animations.BalanceFlip;
					data.Visual.Facing = direction;
				}
				break;
			
			case PlayerNode.Types.Sonic:
				if (!isPanic)
				{
					data.Visual.Animation = data.Visual.Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (data.Visual.Facing != direction)
				{
					data.Visual.Animation = Animations.BalanceTurn;
					data.Visual.Facing = direction;
				}
				else if (data.Visual.Animation != Animations.BalanceTurn)
				{
					data.Visual.Animation = Animations.BalancePanic;
				}
				break;
		}
	}
}