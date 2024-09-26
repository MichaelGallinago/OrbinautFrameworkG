using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Objects.Player.ActionFsm;

namespace OrbinautFrameworkG.Objects.Player.Physics;

public readonly struct Balancing(PlayerData data, IPlayerLogic logic)
{
    public void Balance()
	{
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
		int playerX = Mathf.FloorToInt(onObject.SolidBox.Radius.X - onObject.Position.X + data.Movement.Position.X);
		
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
		CollisionData collision = data.Collision;
		if (collision.OnObject != null) return false;
		
		const Constants.Direction direction = Constants.Direction.Positive;	
		
		if (Angles.GetQuadrant(data.Movement.Angle) > Angles.Quadrant.Down) return true;
		
		Vector2I position = (Vector2I)data.Movement.Position + new Vector2I(0, collision.Radius.Y);
		TileCollider collider = logic.TileCollider;
		collider.SetData(position, collision.TileLayer);
		if (collider.FindDistance(0, 0, true, direction) < 12) return true;
		
		(_, float angleLeft) = collider.FindTile(-collision.Radius.X, 0, true, direction);
		(_, float angleRight) = collider.FindTile(collision.Radius.X, 0, true, direction);
		
		if (float.IsNaN(angleLeft) == float.IsNaN(angleRight)) return true;
		
		int sign = float.IsNaN(angleLeft) ? -1 : 1;
		bool isPanic = collider.FindDistance(-6 * sign, 0, true, direction) >= 12;
		BalanceToDirection((Constants.Direction)sign, isPanic);
		return true;
	}

	private void BalanceToDirection(Constants.Direction direction, bool isPanic)
	{
		IPlayerSprite sprite = data.Sprite;
		VisualData visual = data.Visual;
		switch (data.Node.Type)
		{
			case PlayerNode.Types.Amy or PlayerNode.Types.Tails:
			case PlayerNode.Types.Sonic when data.Super.IsSuper:
				sprite.Animation = Animations.Balance;
				visual.Facing = direction;
				break;
			
			case PlayerNode.Types.Knuckles:
				if (visual.Facing == direction)
				{
					sprite.Animation = Animations.Balance;
				}
				else if (sprite.Animation != Animations.BalanceFlip)
				{
					sprite.Animation = Animations.BalanceFlip;
					visual.Facing = direction;
				}
				break;
			
			case PlayerNode.Types.Sonic:
				if (!isPanic)
				{
					sprite.Animation = visual.Facing == direction ? Animations.Balance : Animations.BalanceFlip;
				}
				else if (visual.Facing != direction)
				{
					sprite.Animation = Animations.BalanceTurn;
					visual.Facing = direction;
				}
				else if (sprite.Animation != Animations.BalanceTurn)
				{
					sprite.Animation = Animations.BalancePanic;
				}
				break;
		}
	}
}