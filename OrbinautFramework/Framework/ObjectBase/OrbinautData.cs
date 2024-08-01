using System;
using Godot;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.ObjectBase;

[GlobalClass]
public partial class OrbinautData : Resource
{
	public Vector2 PreviousPosition { get; set; }
	public HitBox HitBox;
	public SolidBox SolidBox;

	public override void _Ready() => Init();

	protected virtual void Init() {}

	public override void _ExitTree() => ObjectCuller.Local.RemoveFromCulling(this);
	
	
	public bool IsCameraTarget(out ICamera camera) => Views.Local.TargetedCameras.TryGetValue(this, out camera);
	
	//TODO: cleanup
	public bool CheckHitBoxCollision(OrbinautData target, bool isExtraHitBox = false)
	{
		if (!HitBox.IsInteract || !target.HitBox.IsInteract) return false;
		
		var hitBoxColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (isExtraHitBox)
		{
			targetRadius = target.HitBox.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				isExtraHitBox = false;
			}	
			else
			{
				targetOffset = target.HitBox.OffsetExtra;
				hitBoxColor = Color.Color8(0, 0, 220);
			}
		}

		if (!isExtraHitBox)
		{
			targetRadius = target.HitBox.Radius;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0) return false;
			
			targetOffset = target.HitBox.Offset;
			hitBoxColor = Color.Color8(255, 0, 220);
		}
		
		// Calculate bounding boxes for both objects
		Vector2I position = (Vector2I)Position + HitBox.Offset;
		Vector2I boundsNegative = position - HitBox.Radius;
		Vector2I boundsPositive = position + HitBox.Radius;
			
		Vector2I targetPosition = (Vector2I)target.Position + targetOffset;
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
		HitBox.IsInteract = false;
		target.HitBox.IsInteract = false;
		
		return true;
	}

	public bool CheckPlayerHitBoxCollision(PlayerData player, bool isExtraHitBox = false)
	{
		if (!player.IsObjectInteractionEnabled || player.IsHurt) return false;
		return CheckHitBoxCollision(player, isExtraHitBox);
	}

	public bool CheckSolidCollision(Player player, CollisionSensor type)
	{
		if (!player.IsObjectInteractionEnabled) return false;

		// No solid collision data, exit collision check
		if (!player.TouchObjects.TryGetValue(this, out TouchState touchState)) return false;
		
		// Register collision check if debugging
		//TODO: debug collision
		/*if (SharedData.DebugCollision == 3 && ds_list_find_index(dsList, id) == -1)
		{
			var dsList = c_engine.collision.ds_solids_c;
			
			var _rx = SolidData.Radius.X;
			var _ry = SolidData.Radius.Y;
			var _ox = SolidData.Offset.X;
			var _oy = SolidData.Offset.Y;	
		}*/
		
		return type switch
		{
			CollisionSensor.Top => touchState == TouchState.Top,
			CollisionSensor.Bottom => touchState == TouchState.Bottom,
			CollisionSensor.Left => touchState == TouchState.Left,
			CollisionSensor.Right => touchState == TouchState.Right,
			CollisionSensor.Any => touchState != TouchState.None,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public bool CheckPushCollision(PlayerData player)
	{
		//TODO: debug collision
		/*
		if _do_debug
		{
			var _left = x - _rx + _ox;
			var _right = x + _rx + _ox;
			var _width = 4;
						
			ds_list_add(_ds_list, _left, y - _ry + _oy, _left + _width, y + _ry + _oy, _push_colour);
			ds_list_add(_ds_list, _right - _width, y - _ry + _oy, _right, y + _ry + _oy, _push_colour);
		}
		*/
		
		return player.PushObjects.Contains(this);
	}

	private void RemoveFromCulling() => ObjectCuller.Local.RemoveFromCulling(this);
	private void AddToCulling() => ObjectCuller.Local.AddToCulling(this);
}
