using System;
using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Scenes;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class BaseObject : Node2D
{
	public enum CullingType : byte { None, NoBounds, Reset, ResetX, ResetY, Delete, Pause }
	
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
					_culler.AddToCulling(this);
					break;
				default:
					if (value != CullingType.None) break;
					_culler.RemoveFromCulling(this);
					break;
			}

			_culling = value;
		}
	}
	private CullingType _culling;
	
	public new Vector2 Position
	{
		get => _floatPosition;
		set
		{
			base.Position = (Vector2I)value;
			_floatPosition = value;
		}
	}
	private Vector2 _floatPosition;

	public ResetData ResetData { get; private set; }
	public Vector2 PreviousPosition { get; set; }
	public InteractData InteractData;
	public SolidData SolidData;
	
	[UsedImplicitly] private ObjectCuller _culler;
	[UsedImplicitly] private IScene _scene;
	
	public override void _EnterTree()
	{
		Position = base.Position;
		ResetData = new ResetData(Visible, Scale, Position, ZIndex);
		if (Culling != CullingType.None)
		{
			_culler.AddToCulling(this);
		}
	}

	public override void _Ready() => Init();

	protected virtual void Init() {}
	
	public void Reset()
	{
		Position = ResetData.Position;
		Scale = ResetData.Scale;
		Visible = ResetData.IsVisible;
		ZIndex = ResetData.ZIndex;
		Init();
	}

	public override void _ExitTree() => _culler.RemoveFromCulling(this);
	
	public void SetSolid(Vector2I radius, Vector2I offset = default)
	{
		SolidData.Offset = offset;
		SolidData.Radius = radius;
		SolidData.HeightMap = null;
	}
	
	public void SetSolid(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
	{
		SolidData.Radius = new Vector2I(radiusX, radiusY);
		SolidData.Offset = new Vector2I(offsetX, offsetY);
		SolidData.HeightMap = null;
	}
	
	public void SetHitBox(Vector2I radius, Vector2I offset = default)
	{
		InteractData.Radius = radius;
		InteractData.Offset = offset;
	}
	
	public void SetHitBox(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
	{
		InteractData.Radius = new Vector2I(radiusX, radiusY);
		InteractData.Offset = new Vector2I(offsetX, offsetY);
	}

	public void SetHitBoxExtra(Vector2I radius, Vector2I offset = default)
	{
		InteractData.OffsetExtra = offset;
		InteractData.RadiusExtra = radius;
	}
	
	public void SetHitBoxExtra(int radiusX, int radiusY, int offsetX = 0, int offsetY = 0)
	{
		InteractData.RadiusExtra = new Vector2I(radiusX, radiusY);
		InteractData.OffsetExtra = new Vector2I(offsetX, offsetY);
	}
	
	public bool IsCameraTarget(out ICamera camera) => _scene.Views.TargetedCameras.TryGetValue(this, out camera);
	
	public bool CheckHitBoxCollision(BaseObject target, bool isExtraHitBox = false)
	{
		if (!InteractData.IsInteract || !target.InteractData.IsInteract) return false;
		
		var hitBoxColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (isExtraHitBox)
		{
			targetRadius = target.InteractData.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				isExtraHitBox = false;
			}	
			else
			{
				targetOffset = target.InteractData.OffsetExtra;
				hitBoxColor = Color.Color8(0, 0, 220);
			}
		}

		if (!isExtraHitBox)
		{
			targetRadius = target.InteractData.Radius;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0) return false;
			
			targetOffset = target.InteractData.Offset;
			hitBoxColor = Color.Color8(255, 0, 220);
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

	private void RemoveFromCulling() => _culler.RemoveFromCulling(this);
	private void AddToCulling() => _culler.AddToCulling(this);
}
