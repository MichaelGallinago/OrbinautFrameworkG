using System;
using Godot;
using OrbinautFrameworkG.Framework.MathUtilities;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Common.Spikes;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Physics;
using OrbinautFrameworkG.Objects.Player.Sprite;
using static OrbinautFrameworkG.Framework.StaticStorages.Constants;
using Debug = OrbinautFrameworkG.Framework.DebugModule.Debug;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public struct ObjectInteraction(PlayerData data, IPlayerLogic logic)
{
	private const int GripY = 4;
	
	private SolidObjectData _solidObjectData;
	
	public void ClearPush(object target)
	{
		VisualData visual = data.Visual;
		if (visual.SetPushBy != target) return;
		visual.SetPushBy = null;
		
		if (data.Sprite.Animation is not (Animations.Spin or Animations.SpinDash))
		{
			data.Sprite.Animation = Animations.Move;
		}
	}

	public void ActSolid(ISolid target, SolidType type, AttachType attachType = AttachType.Default)
	{
		SolidBox targetBox = target.SolidBox;
		data.Collision.TouchObjects.TryAdd(target, TouchState.None);
		data.Collision.PushObjects.Add(target);
		
		if (!data.State.IsObjectInteractable()) return;
		if (data.Node.SolidBox.Radius.X <= 0 || data.Node.SolidBox.Radius.Y <= 0) return;
		if (targetBox.Radius.X <= 0 || targetBox.Radius.Y <= 0) return;
		
		_solidObjectData = new SolidObjectData(target, type, attachType, ExtraSize);
		int slopeOffset = CalculateSlopeOffset();
		
		RegisterCollisionCheck();
		
		if (data.Collision.OnObject == target)
		{
			StandingOnObject(slopeOffset);
			return;
		}
		
		if (type != SolidType.Top)
		{
			CollideWithRegularObject(slopeOffset);
		}
		else if (data.Movement.Velocity.Y >= 0f)
		{
			CollideWithPlatformObject();
		}
	}
	
	public bool CheckSolidCollision(ISolid target, CollisionSensor type)
	{
		if (!data.State.IsObjectInteractable()) return false;
		
		// No solid collision data, exit collision check
		if (!data.Collision.TouchObjects.TryGetValue(target, out TouchState touchState)) return false;
		
		// Register collision check if debugging
		//TODO: debug collision
		/*if (SharedData.DebugCollision == 3 && ds_list_find_index(dsList, id) == -1)
		{
			var dsList = c_engine.collision.ds_solids_c;

			var _rx = solidBox.Radius.X;
			var _ry = solidBox.Radius.Y;
			var _ox = solidBox.Offset.X;
			var _oy = solidBox.Offset.Y;
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
	
	public bool CheckPushCollision(IPlayer player)
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
		
		return player.Data.Collision.PushObjects.Contains(player.Data.Node);
	}

// Extend the radiuses for better & fair solid collision (if enabled)
#if BETTER_SOLID_COLLISION
	private Vector2I ExtraSize => new(data.Node.SolidBox.Radius.X, GripY);
#else
	private static Vector2I ExtraSize => Vector2I.Zero;
#endif
	
	private int CalculateSlopeOffset()
	{
		// Adjust slope offset based on height map
		short[] heightMap = _solidObjectData.Target.SolidBox.HeightMap;
		
		if (heightMap is not { Length: > 0 }) return 0;

		int sign = _solidObjectData.Target.Scale.X >= 0 ? 1 : -1;
		int distance = sign * Mathf.FloorToInt(data.Node.Position.X - _solidObjectData.Position.X);
		
		int index = Math.Clamp(distance + _solidObjectData.Target.SolidBox.Radius.X, 0, heightMap.Length - 1);
		
		return (_solidObjectData.Target.SolidBox.Radius.Y - heightMap[index]) * (int)_solidObjectData.Target.Scale.Y;
	}
	
	private void RegisterCollisionCheck()
	{
		//if (Debug.Instance.SensorType != Debug.SensorTypes.SolidBox) return;
		// TODO: debug
		/*
		// Register collision check if debugging
		if global.debug_collision == 3
		{
			var _ds_list = c_framework.collision.ds_solid_size;
			var _solid_colour = $00FFFF;
			
			if ds_list_find_index(_ds_list, _player) == -1
			{
				ds_list_add(_ds_list, _px - _pw, _py - _ph, _px + _pw, _py + _ph, _solid_colour, _player);
			}
			
			if ds_list_find_index(_ds_list, id) == -1
			{
				ds_list_add(_ds_list, _obj_x - _obj_w, _obj_y - _obj_h + _slope_offset, _obj_x + _obj_w, _obj_y + _obj_h + _slope_offset, _solid_colour, id);
			}
		}
		*/
	}
	
	private void StandingOnObject(int slopeOffset)
	{
		ISolid target = _solidObjectData.Target;
		SolidBox targetSolidBox = target.SolidBox;
		Vector2 targetPosition = _solidObjectData.Position;
		
		CollisionData collision = data.Collision;
		collision.TouchObjects[target] = TouchState.Top;
		
		Vector2 position = data.Node.Position;
		position = targetPosition + new Vector2(
			position.X - target.PreviousPosition.X, 
			slopeOffset - targetSolidBox.Radius.Y - data.Node.SolidBox.Radius.Y - 1);
		data.Node.Position = position;
		
		float distance = position.X - targetPosition.X;
		float relativeX = Math.Abs(MathF.Floor(distance)) - targetSolidBox.Radius.X;
		
		if (_solidObjectData.Type == SolidType.Top ? 
			    relativeX <= _solidObjectData.ExtraSize.X : relativeX < data.Node.SolidBox.Radius.X + 1) return;
			
		// Reset touch flags and player's on-object status if they are out of bounds
		collision.TouchObjects[target] = TouchState.None;
		collision.OnObject = null;
	}
	
	private void CollideWithRegularObject(int slopeOffset)
	{
		// Calculate distances for collision detection
		Vector2I combinedSize = _solidObjectData.Target.SolidBox.Radius + data.Node.SolidBox.Radius;
		combinedSize.X++;
		
		Vector2 position = data.Node.Position;
		Vector2I distance = (Vector2I)(position - _solidObjectData.Position) + combinedSize;
		distance.Y += GripY - slopeOffset;
		
		// Check if player is out of bounds
		if (distance.X < 0 || distance.X > combinedSize.X * 2 || 
		    distance.Y < 0 || distance.Y > combinedSize.Y * 2 + _solidObjectData.ExtraSize.Y)
		{
			ClearPush(_solidObjectData.Target);
			return;
		}
		
		Vector2I flooredDistance = (Vector2I)position - (Vector2I)_solidObjectData.Position;
		
		Vector2I clip = distance - new Vector2I(
			flooredDistance.X < 0 ? 0 : combinedSize.X * 2, 
			flooredDistance.Y < 0 ? 0 : combinedSize.Y * 2 + GripY);

		Vector2I clipAbsolute = clip.Abs();
		bool isLimitedClip = clipAbsolute.Y <= 4;
		
#if S3_PHYSICS || SK_PHYSICS
		if (isLimitedClip || clipAbsolute.X >= clipAbsolute.Y)
		{
			if (CollideVertically(clip.Y)) return;
		}
		
		CollideHorizontally(flooredDistance.X, clip.X);
#else
		if (clipAbsolute.X >= clipAbsolute.Y)
		{
			if (CollideVertically(clip.Y)) return;
		}
		
		CollideHorizontally(isLimitedClip, flooredDistance.X, clip.X);
#endif
	}

#if S3_PHYSICS || SK_PHYSICS
	private void CollideHorizontally(int flooredDistanceX, int clipX)
	{
#else
	private void CollideHorizontally(bool isLimitedClip, int flooredDistanceX, int clipX)
	{
		if (isLimitedClip)
		{
			ClearPush(_solidObjectData.Target); 
			return;
		}
#endif

		TouchState touchState = flooredDistanceX < 0 ? TouchState.Left : TouchState.Right;
		data.Collision.TouchObjects[_solidObjectData.Target] = touchState;

		UpdatePushingStatus(clipX, flooredDistanceX);
	}
	
	private void UpdatePushingStatus(int clipX, int flooredDistanceX)
	{
		MovementData movement = data.Movement;
		VisualData visual = data.Visual;
		bool isFaced = movement.IsGrounded && Math.Sign((int)visual.Facing) == Math.Sign(-flooredDistanceX);
		visual.SetPushBy = isFaced ? _solidObjectData.Target : null;
		
		if (clipX != 0 && Math.Sign(clipX) == Math.Sign(movement.Velocity.X))
		{
			movement.GroundSpeed = 0f;
			movement.Velocity.X = 0f;
			
			if (movement.IsGrounded)
			{
				data.Collision.PushObjects.Add(_solidObjectData.Target);
			}
		}

		data.Node.Position = data.Node.Position.AddX(-clipX);
	}
	
	private bool CollideVertically(int clipY)
	{
		switch (clipY)
		{
			case < 0: 
				return CollideUpward(clipY);
			
			case < 16 when _solidObjectData.Type != SolidType.Sides: 
				CollideDownward(clipY);
				return true;
			
			default: 
				ClearPush(_solidObjectData.Target);
				return true;
		}
	}
	
	private bool CollideUpward(int clipY)
	{
		if (_solidObjectData.Type is SolidType.ItemBox or SolidType.Sides) return false;

		if (data.Movement.Velocity.Y == 0f && data.Movement.IsGrounded)
		{
			return CrushPlayer(clipY);
		}

		HandleUpwardCollision(clipY);
		return true;
	}
	
	private bool CrushPlayer(int clipY)
	{
		if (Math.Abs(clipY) < 16) return false;
		logic.Kill();
		return true;
	}
	
	private void HandleUpwardCollision(int clipY)
	{
		MovementData movement = data.Movement;
		if (movement.Velocity.Y < 0f)
		{
#if S3_PHYSICS || SK_PHYSICS
			if (!movement.IsGrounded)
			{
				movement.GroundSpeed = 0f;
			}
#endif
			data.Node.Position = data.Node.Position.AddY(-clipY);
			movement.Velocity.Y = 0f;
		}

		data.Collision.TouchObjects[_solidObjectData.Target] = TouchState.Bottom;
	}
	
	private void CollideDownward(int clipY)
	{
		if (data.Movement.Velocity.Y < 0f) return;
		
		float relativeX = Math.Abs(MathF.Floor(data.Node.Position.X - _solidObjectData.Position.X));
		if (relativeX > _solidObjectData.Target.SolidBox.Radius.X + _solidObjectData.ExtraSize.X) return;
		
		data.Collision.TouchObjects[_solidObjectData.Target] = TouchState.Top;
		AttachToObject(clipY - GripY);
	}
	
	private void CollideWithPlatformObject()
	{
		var position = (Vector2I)data.Node.Position;
		var targetPosition = (Vector2I)_solidObjectData.Position;
		Vector2I targetRadius = _solidObjectData.Target.SolidBox.Radius;
		
		if (Math.Abs(position.X - targetPosition.X) > targetRadius.X + _solidObjectData.ExtraSize.X) return;
		
		int clipY = targetPosition.Y - targetRadius.Y - position.Y - data.Node.SolidBox.Radius.Y - GripY;
		if (clipY is < -16 or >= 0) return;
		
		data.Collision.TouchObjects[_solidObjectData.Target] = TouchState.Top;
		AttachToObject(-GripY - clipY);
	}
	
	private void AttachToObject(int distance)
	{
		switch (_solidObjectData.AttachType)
		{
			case AttachType.None:
				return;
			
			case AttachType.ResetPlayer:
				logic.ResetData();
				logic.Action = ActionFsm.States.Default;
				break;
		}
		
		data.Node.Position = data.Node.Position.AddX(-distance - 1);
		
		MovementData movement = data.Movement;
		movement.GroundSpeed = movement.Velocity.X;
		movement.Velocity.Y = 0f;
		movement.Angle = 0f;
		
		data.Collision.OnObject = _solidObjectData.Target;
		
		if (movement.IsGrounded) return;
		logic.Land();
	}
}
