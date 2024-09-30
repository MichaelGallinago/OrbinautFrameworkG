using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Framework.ObjectBase;

[GlobalClass]
public partial class HitBox : Resource
{
    [Export] public Vector2I Radius { get; private set; }
    [Export] public Vector2I Offset { get; private set; }
    [Export] public Vector2I RadiusExtra { get; private set; }
    [Export] public Vector2I OffsetExtra { get; private set; }
    
    public void Set(Vector2I radius, Vector2I offset = default)
    {
        Radius = radius;
        Offset = offset;
    }
	
    public void Set(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        Radius = new Vector2I(radiusX, radiusY);
        Offset = new Vector2I(offsetX, offsetY);
    }

    public void SetExtra(Vector2I radius, Vector2I offset = default)
    {
        RadiusExtra = radius;
        OffsetExtra = offset;
    }
	
    public void SetExtra(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        RadiusExtra = new Vector2I(radiusX, radiusY);
        OffsetExtra = new Vector2I(offsetX, offsetY);
    }
    
    public bool CheckCollision(HitBox target, Vector2I targetPosition, Vector2I position, bool isExtra = false)
    {
	    if (!target.GetData(isExtra, out Vector2I targetOffset, out Vector2I targetRadius)) return false;
		
		// Calculate bounding boxes for both objects
		position += Offset;
		Vector2I boundsNegative = position - Radius;
		Vector2I boundsPositive = position + Radius;
		
		targetPosition += targetOffset;
		Vector2I targetBoundsNegative = targetPosition - targetRadius;
		Vector2I targetBoundsPositive = targetPosition + targetRadius;
		
		// Check for horizontal and vertical overlap between HitBoxes
		if (targetBoundsPositive.X < boundsNegative.X || targetBoundsNegative.X > boundsPositive.X) return false;
		if (targetBoundsPositive.Y < boundsNegative.Y || targetBoundsNegative.Y > boundsPositive.Y) return false;
		
		return true;
	}

    private bool GetData(bool isExtra, out Vector2I offset, out Vector2I radius)
	{
		if (isExtra)
		{
			radius = RadiusExtra;
			
			if (radius is { X: > 0, Y: > 0 })
			{
				offset = OffsetExtra;
				return true;
			}
		}

		radius = Radius;
		
		if (radius is { X: > 0, Y: > 0 })
		{
			offset = Offset;
			return true;
		}
		
		offset = default;
		return false;
	}
    
	public bool CheckPlayerCollision(PlayerData player, Vector2I position, bool isExtraHitBox = false)
	{
		if (player.State != PlayerStates.Control) return false;
		IPlayerNode node = player.Node;
		return CheckCollision(node.HitBox, (Vector2I)node.Position, position, isExtraHitBox);
	}
}
    