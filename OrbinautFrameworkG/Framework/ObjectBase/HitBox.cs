using Godot;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Framework.ObjectBase;

[GlobalClass]
public partial class HitBox : Resource
{
    public bool IsInteract { get; private set; }
    [Export] public Vector2I Radius { get; private set; }
    [Export] public Vector2I Offset { get; private set; }
    [Export] public Vector2I RadiusExtra { get; private set; }
    [Export] public Vector2I OffsetExtra { get; private set; }

    public HitBox(bool isInteract, Vector2I radius, Vector2I offset, Vector2I radiusExtra, Vector2I offsetExtra)
    {
        IsInteract = isInteract;
        Radius = radius;
        Offset = offset;
        RadiusExtra = radiusExtra;
        OffsetExtra = offsetExtra;
    }
    
    public HitBox() : this(false, default, default, default, default) {}
    
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
        OffsetExtra = offset;
        RadiusExtra = radius;
    }
	
    public void SetExtra(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
    {
        RadiusExtra = new Vector2I(radiusX, radiusY);
        OffsetExtra = new Vector2I(offsetX, offsetY);
    }
    
    public bool CheckCollision(HitBox target, Vector2I position, Vector2I targetPosition, bool isExtraHitBox = false)
	{
		if (!IsInteract || !target.IsInteract) return false;
		
		var hitBoxColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (isExtraHitBox)
		{
			targetRadius = target.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				isExtraHitBox = false;
			}	
			else
			{
				targetOffset = target.OffsetExtra;
				hitBoxColor = Color.Color8(0, 0, 220);
			}
		}

		if (!isExtraHitBox)
		{
			targetRadius = target.Radius;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0) return false;
			
			targetOffset = target.Offset;
			hitBoxColor = Color.Color8(255, 0, 220);
		}
		
		// Calculate bounding boxes for both objects
		position += Offset;
		Vector2I boundsNegative = position - Radius;
		Vector2I boundsPositive = position + Radius;
		targetPosition += targetOffset;
		Vector2I targetBoundsNegative = targetPosition - targetRadius;
		Vector2I targetBoundsPositive = targetPosition + targetRadius;
			
		// Register collision check if debugging
		//TODO: debug collision
		/*
		if global.debug_collision == 2
		{
			var _ds_list = c_framework.collision.ds_interact;
			
			if ds_list_find_index(_ds_list, _target) == -1
			{
				ds_list_add(_ds_list, _target_l, _target_t, _target_r, _target_b, _hitbox_colour, _target);
			}
			
			if ds_list_find_index(_ds_list, id) == -1
			{
				ds_list_add(_ds_list, _this_l, _this_t, _this_r, _this_b, _hitbox_colour, id);
			}
		}*/
		
		// Check for collision in the x-axis
		if (targetBoundsPositive.X < boundsNegative.X || targetBoundsNegative.X > boundsPositive.X) return false;
		
		// Check for collision in the y-axis
		if (targetBoundsPositive.Y < boundsNegative.Y || targetBoundsNegative.Y > boundsPositive.Y) return false;
		
		// Objects should no longer interact with any other object this step
		IsInteract = false;
		target.IsInteract = false;
		
		return true;
	}
    
	public bool CheckPlayerCollision(PlayerData player, Vector2I position, bool isExtraHitBox = false)
	{
		if (player.State != PlayerStates.Control) return false;
		IPlayerNode node = player.Node;
		return CheckCollision(node.HitBox, (Vector2I)node.Position, position, isExtraHitBox);
	}
}
    