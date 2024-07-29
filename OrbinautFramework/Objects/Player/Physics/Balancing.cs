using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;

namespace OrbinautFramework3.Objects.Player.Physics;

public struct Balancing
{
    public void Balance()
	{
		if (!IsGrounded || IsSpinning) return;
		if (GroundSpeed != 0 || Action is Actions.SpinDash or Actions.Dash) return;
		
		// Don't allow player to duck or look up
		if (SharedData.PlayerPhysics == PhysicsTypes.SK && (Input.Down.Down || Input.Down.Up)) return;
		
		if (BalanceOnTiles()) return;
		BalanceOnObject();
	}
	
	private void BalanceOnObject()
	{
		if (OnObject == null) return;
		// TODO: check IsInstanceValid == instance_exist
		if (!GodotObject.IsInstanceValid(OnObject) || OnObject.SolidData.NoBalance) return;
		
		const int leftEdge = 2;
		const int panicOffset = 4;
		
		int rightEdge = OnObject.SolidData.Radius.X * 2 - leftEdge;
		int playerX = Mathf.FloorToInt(OnObject.SolidData.Radius.X - OnObject.Position.X + Position.X);
		
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
		if (OnObject != null) return false;
		
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(Angle) > Angles.Quadrant.Down) return true;
		TileCollider.SetData((Vector2I)Position + new Vector2I(0, Radius.Y), TileLayer);
		
		if (TileCollider.FindDistance(0, 0, true, direction) < 12) return true;
		
		(_, float angleLeft) = TileCollider.FindTile(-Radius.X, 0, true, direction);
		(_, float angleRight) = TileCollider.FindTile(Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) == float.IsNaN(angleRight)) return true;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = TileCollider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
		return true;
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (Type)
		{
			case Types.Amy or Types.Tails:
			case Types.Sonic when IsSuper:
				Animation = Animations.Balance;
				Facing = direction;
				break;
			
			case Types.Knuckles:
				if (Facing == direction)
				{
					Animation = Animations.Balance;
				}
				else if (Animation != Animations.BalanceFlip)
				{
					Animation = Animations.BalanceFlip;
					Facing = direction;
				}
				break;
			
			case Types.Sonic:
				if (!isPanic)
				{
					Animation = Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (Facing != direction)
				{
					Animation = Animations.BalanceTurn;
					Facing = direction;
				}
				else if (Animation != Animations.BalanceTurn)
				{
					Animation = Animations.BalancePanic;
				}
				break;
		}
	}
}