using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Objects.Player;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class BaseObject : Node2D
{
	public enum BehaviourType : byte { NoBounds, Reset, Pause, Delete, Unique }
	[Export] public BehaviourType Behaviour { get; set; }

	public static List<BaseObject> Objects { get; } = [];
	public ObjectRespawnData RespawnData { get; private set; }
	public SolidData SolidData { get; } = new();
	public Vector2 PreviousPosition { get; set; }

	public InteractData InteractData = new();
	
	public override void _EnterTree()
	{
		Objects.Add(this);
		RespawnData = new ObjectRespawnData(Position, Scale, Visible, ZIndex);
	}

	public override void _ExitTree() => Objects.Remove(this);
	
	public void ResetZIndex() => ZIndex = RespawnData.ZIndex;
	public void SetBehaviour(BehaviourType behaviour)
	{
		if (Behaviour == BehaviourType.Delete) return;
		Behaviour = behaviour;
	}
	
	public virtual void Reset()
	{
		Position = RespawnData.Position;
		Scale = RespawnData.Scale;
		Visible = RespawnData.IsVisible;
		ZIndex = RespawnData.ZIndex;
	}
    
	public void SetSolid(Vector2I radius, Vector2I offset = default)
	{
		SolidData.Radius = radius;
		SolidData.Offset = offset;
		SolidData.HeightMap = null;
	}

	public void SetHitbox(Vector2I radius, Vector2I offset = default)
	{
		InteractData.Radius = radius;
		InteractData.Offset = offset;
	}

	public void SetHitboxExtra(Vector2I radius, Vector2I offset = default)
	{
		InteractData.RadiusExtra = radius;
		InteractData.OffsetExtra = offset;
	}
	
	public bool CheckCollision(BaseObject target, CollisionSensor type)
	{
		if (target is Player { ObjectInteraction: false }) return false;

		return type switch
		{
			CollisionSensor.SolidPush => CheckPushCollision(target),
			CollisionSensor.Hitbox => CheckHitboxCollision(target, type),
			CollisionSensor.HitboxExtra => CheckHitboxCollision(target, type),
			_ => CheckSolidCollision(target, type)
		};
	}
	
	private bool CheckHitboxCollision(BaseObject target, CollisionSensor type)
	{
		if (!InteractData.IsInteract || !target.InteractData.IsInteract) return false;
		var hitboxColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (type == CollisionSensor.HitboxExtra)
		{
			targetRadius = target.InteractData.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				type = CollisionSensor.Hitbox;	
			}	
			else
			{
				targetOffset = target.InteractData.OffsetExtra;
				hitboxColor = Color.Color8(0, 0, 220);
			}
		}

		if (type == CollisionSensor.Hitbox)
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
			CollisionSensor.Hitbox or CollisionSensor.HitboxExtra or CollisionSensor.SolidPush => false,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	private bool CheckPushCollision(BaseObject target)
	{
		return target is Player player && player.PushObjects.Contains(this);
	}
}
