using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Objects.Player.ActionFsm;
using OrbinautNode = OrbinautFramework3.Framework.ObjectBase.AbstractTypes.OrbinautNode;

namespace OrbinautFramework3.Objects.Player.Physics;

public readonly struct Balancing(PlayerData data, IPlayerLogic logic)
{
    public void Balance()
	{
		if (!data.Movement.IsGrounded || data.Movement.IsSpinning) return;
		if (data.Movement.GroundSpeed != 0f || logic.Action is States.SpinDash or States.Dash) return;
		
#if SK_PHYSICS
		// Don't allow player to duck or look up
		if (data.Input.Down.Down || data.Input.Down.Up) return;
#endif
		
		if (BalanceOnTiles()) return;
		BalanceOnObject();
	}
	
	private void BalanceOnObject()
	{
		ISolid onObject = data.Collision.OnObject;
		if (!onObject.IsInstanceValid()) return;
		if (onObject.SolidBox.NoBalance) return;
		
		const int leftEdge = 2;
		const int panicOffset = 4;
		
		int rightEdge = onObject.SolidBox.Radius.X * 2 - leftEdge;
		int playerX = Mathf.FloorToInt(onObject.SolidBox.Radius.X - onObject.Position.X + data.Node.Position.X);
		
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
		logic.TileCollider.SetData(
			(Vector2I)data.Node.Position + new Vector2I(0, data.Collision.Radius.Y), 
			data.Collision.TileLayer);
		
		if (logic.TileCollider.FindDistance(0, 0, true, direction) < 12) return true;
		
		(_, float angleLeft) = logic.TileCollider.FindTile(-data.Collision.Radius.X, 0, true, direction);
		(_, float angleRight) = logic.TileCollider.FindTile(data.Collision.Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) == float.IsNaN(angleRight)) return true;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = logic.TileCollider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
		return true;
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		switch (data.Node.Type)
		{
			case PlayerNode.Types.Amy or PlayerNode.Types.Tails:
			case PlayerNode.Types.Sonic when data.Super.IsSuper:
				data.Sprite.Animation = Animations.Balance;
				data.Visual.Facing = direction;
				break;
			
			case PlayerNode.Types.Knuckles:
				if (data.Visual.Facing == direction)
				{
					data.Sprite.Animation = Animations.Balance;
				}
				else if (data.Sprite.Animation != Animations.BalanceFlip)
				{
					data.Sprite.Animation = Animations.BalanceFlip;
					data.Visual.Facing = direction;
				}
				break;
			
			case PlayerNode.Types.Sonic:
				if (!isPanic)
				{
					data.Sprite.Animation = data.Visual.Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (data.Visual.Facing != direction)
				{
					data.Sprite.Animation = Animations.BalanceTurn;
					data.Visual.Facing = direction;
				}
				else if (data.Sprite.Animation != Animations.BalanceTurn)
				{
					data.Sprite.Animation = Animations.BalancePanic;
				}
				break;
		}
	}
}