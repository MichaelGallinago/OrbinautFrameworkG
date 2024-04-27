using System;
using Godot;
using OrbinautFramework3.Objects.Player;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class BaseObject : Node2D
{
	public enum CullingType : byte { None, NoBounds, Reset, ResetX, ResetY, Delete }
	
	[Export] public CullingType Culling
	{
		get => _culling;
		set
		{ 
			switch (_culling)
			{
				case CullingType.Delete:
					return;
				case CullingType.None:
					FrameworkData.CurrentScene.Culler.AddToCulling(this);
					break;
				default:
					if (value != CullingType.None) break;
					FrameworkData.CurrentScene.Culler.RemoveFromCulling(this);
					break;
			}

			_culling = value;
		}
	}
	private CullingType _culling;

	public ResetData ResetData { get; private set; }
	public Vector2 PreviousPosition { get; set; }
	public InteractData InteractData;
	public SolidData SolidData;
	
	public override void _EnterTree()
	{
		if (Culling != CullingType.None)
		{
			FrameworkData.CurrentScene.Culler.AddToCulling(this);
		}
	}
	
	public virtual void Reset()
	{
		Position = ResetData.Position;
		Scale = ResetData.Scale;
		Visible = ResetData.IsVisible;
		ZIndex = ResetData.ZIndex;
	}

	public override void _ExitTree() => FrameworkData.CurrentScene.Culler.RemoveFromCulling(this);
    
	public void SetSolid(Vector2I radius, Vector2I offset)
	{
		SolidData.Offset = offset;
		SetSolid(radius);
	}
	
	public void SetSolid(Vector2I radius)
	{
		SolidData.Radius = radius;
		SolidData.HeightMap = null;
	}
	
	public void SetHitBox(Vector2I radius, Vector2I offset)
	{
		InteractData.Offset = offset;
		SetHitBox(radius);
	}
	
	public void SetHitBox(Vector2I radius)
	{
		InteractData.Radius = radius;
	}

	public void SetHitBoxExtra(Vector2I radius, Vector2I offset)
	{
		InteractData.OffsetExtra = offset;
		SetHitBoxExtra(radius);
	}
	
	public void SetHitBoxExtra(Vector2I radius)
	{
		InteractData.RadiusExtra = radius;
	}
	
	public bool CheckCollision(BaseObject target, CollisionSensor type)
	{
		if (target is Player { ObjectInteraction: false }) return false;

		return type switch
		{
			CollisionSensor.SolidPush => CheckPushCollision(target),
			CollisionSensor.HitBox => CheckHitBoxCollision(target, type),
			CollisionSensor.HitBoxExtra => CheckHitBoxCollision(target, type),
			_ => CheckSolidCollision(target, type)
		};
	}
	
	private bool CheckHitBoxCollision(BaseObject target, CollisionSensor type)
	{
		if (!InteractData.IsInteract || !target.InteractData.IsInteract) return false;
		var hitboxColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (type == CollisionSensor.HitBoxExtra)
		{
			targetRadius = target.InteractData.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				type = CollisionSensor.HitBox;	
			}	
			else
			{
				targetOffset = target.InteractData.OffsetExtra;
				hitboxColor = Color.Color8(0, 0, 220);
			}
		}

		if (type == CollisionSensor.HitBox)
		{
			targetRadius = target.InteractData.Radius;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0) return false;
			
			targetOffset = target.InteractData.Offset;
			hitboxColor = Color.Color8(255, 0, 220);
		}
		
		// Calculate bounding boxes for both objects
		Vector2I position = (Vector2I)Position + InteractData.Offset;
		Vector2I boundsNegative = position - InteractData.Radius;
		Vector2I boundsPositive = position + InteractData.Radius;
			
		Vector2I targetPosition = (Vector2I)target.Position + targetOffset;
		Vector2I targetBoundsNegative = targetPosition - targetRadius;
		Vector2I targetBoundsPositive = targetPosition + targetRadius;
			
		// Register collision check if debugging
		//TODO: debug collision
		/*if (SharedData.DebugCollision == 2)
		{
			var _ds_list = c_engine.collision.ds_interact;
			
			ds_list_add(_ds_list, _target_l, _target_t, _target_r, _target_b, _hitbox_colour);
			ds_list_add(_ds_list, _this_l, _this_t, _this_r, _this_b, _hitbox_colour);
		}*/
		
		// Check for collision in the x-axis
		if (targetBoundsPositive.X < boundsNegative.X || targetBoundsNegative.X > boundsPositive.X) return false;
		
		// Check for collision in the y-axis
		if (targetBoundsPositive.Y < boundsNegative.Y || targetBoundsNegative.Y > boundsPositive.Y) return false;
		
		// This objects should not interact with any other objects this frame anymore
		InteractData.IsInteract = false;
		target.InteractData.IsInteract = false;
			
		return true;
	}

	private bool CheckSolidCollision(BaseObject target, CollisionSensor type)
	{
		if (target is not Player player) return false;

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
			CollisionSensor.SolidU => touchState == TouchState.Up,
			CollisionSensor.SolidD => touchState == TouchState.Down,
			CollisionSensor.SolidL => touchState == TouchState.Left,
			CollisionSensor.SolidR => touchState == TouchState.Right,
			CollisionSensor.SolidAny => touchState != TouchState.None,
			CollisionSensor.HitBox or CollisionSensor.HitBoxExtra or CollisionSensor.SolidPush => false,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	private bool CheckPushCollision(BaseObject target)
	{
		return target is Player player && player.PushObjects.Contains(this);
	}

	private void RemoveFromCulling() => FrameworkData.CurrentScene.Culler.RemoveFromCulling(this);
	private void AddToCulling() => FrameworkData.CurrentScene.Culler.AddToCulling(this);
}
