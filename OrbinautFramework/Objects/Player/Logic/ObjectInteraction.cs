using System;
using Godot;
using Godot.Collections;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Player.Logic;

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

	public void ActSolid(ISolid target, SolidType type, bool isFullRoutine = true)
	{
		SolidBox targetBox = target.SolidBox;
		data.Collision.TouchObjects.TryAdd(targetBox, TouchState.None);
		data.Collision.PushObjects.Add(targetBox);
		
		if (!data.Collision.IsObjectInteractionEnabled) return;
		if (data.Node.SolidBox.Radius.X <= 0 || data.Node.SolidBox.Radius.Y <= 0) return;
		if (targetBox.Radius.X <= 0 || targetBox.Radius.Y <= 0) return;
		
		_solidObjectData = new SolidObjectData(target, type, GetExtraSize());
		int slopeOffset = CalculateSlopeOffset();
		
		RegisterCollisionCheck();
		
		if (data.Collision.OnObject == target)
		{
			StandingOnObject(slopeOffset);
			return;
		}
		
		if (!isFullRoutine) return;
		
		if (type != SolidType.Top)
		{
			CollideWithRegularObject(slopeOffset);
		}
		else if (data.Movement.Velocity.Y >= 0f)
		{
			CollideWithPlatformObject();
		}
	}
	
	public bool CheckSolidCollision(SolidBox solidBox, CollisionSensor type)
	{
		if (!data.Collision.IsObjectInteractionEnabled) return false;

		// No solid collision data, exit collision check
		if (!data.Collision.TouchObjects.TryGetValue(solidBox, out TouchState touchState)) return false;
		
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
		
		return player.Data.Collision.PushObjects.Contains(data.Node.SolidBox);
	}
	
	private Vector2I GetExtraSize()
	{
		// Extend the radiuses for better & fair solid collision (if enabled)
		return SharedData.BetterSolidCollision ? new Vector2I(data.Node.SolidBox.Radius.X, GripY) : Vector2I.Zero;
	}
	
	private int CalculateSlopeOffset()
	{
		// Adjust slope offset based on height map
		Array<short> heightMap = _solidObjectData.Target.SolidBox.HeightMap;
		
		if (heightMap is not { Count: > 0 }) return 0;

		int distance = 
			Mathf.FloorToInt(data.Node.Position.X - _solidObjectData.Position.X) * (_solidObjectData.Target.Scale.X >= 0 ? 1 : -1);
		
		int index = Math.Clamp(distance + _solidObjectData.Target.SolidBox.Radius.X, 0, heightMap.Count - 1);
		
		return (_solidObjectData.Target.SolidBox.Radius.Y - heightMap[index]) * (int)_solidObjectData.Target.Scale.Y;
	}
	
	private void RegisterCollisionCheck()
	{
		if (SharedData.SensorDebugType != SharedData.SensorDebugTypes.SolidBox) return;
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
		data.Collision.TouchObjects[_solidObjectData.Target.SolidBox] = TouchState.Top;
		
		data.Node.Position = _solidObjectData.Position + new Vector2(
			data.Node.Position.X - _solidObjectData.Target.PreviousPosition.X, 
			slopeOffset - _solidObjectData.Target.SolidBox.Radius.Y - data.Node.SolidBox.Radius.Y - 1);

		float relativeX = Math.Abs(MathF.Floor(data.Node.Position.X - _solidObjectData.Position.X)) - 
		                  _solidObjectData.Target.SolidBox.Radius.X;

		if (_solidObjectData.Type == SolidType.Top ? 
			    relativeX <= _solidObjectData.ExtraSize.X : relativeX < data.Node.SolidBox.Radius.X + 1) return;
			
		// Reset touch flags and player's on-object status if they are out of bounds
		data.Collision.TouchObjects[_solidObjectData.Target.SolidBox] = TouchState.None;
		data.Collision.OnObject = null;
	}
	
	private void CollideWithRegularObject(int slopeOffset)
	{
		// Calculate distances for collision detection
		Vector2I combinedSize = _solidObjectData.Target.SolidBox.Radius + data.Node.SolidBox.Radius;
		combinedSize.X++;
		
		Vector2I distance = (Vector2I)(data.Node.Position - _solidObjectData.Position) + combinedSize;
		distance.Y += GripY - slopeOffset;
		
		// Check if player is out of bounds
		if (distance.X < 0 || distance.X > combinedSize.X * 2 || 
		    distance.Y < 0 || distance.Y > combinedSize.Y * 2 + _solidObjectData.ExtraSize.Y)
		{
			ClearPush(_solidObjectData.Target);
			return;
		}

		Vector2I flooredDistance = (Vector2I)data.Node.Position - (Vector2I)_solidObjectData.Position;
		
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
		
		data.Collision.TouchObjects[_solidObjectData.Target.SolidBox] = 
			flooredDistanceX < 0 ? TouchState.Left : TouchState.Right;

		UpdatePushingStatus(clipX, flooredDistanceX);
	}
	
	private void UpdatePushingStatus(int clipX, int flooredDistanceX)
	{
		data.Visual.SetPushBy = data.Movement.IsGrounded && 
			Math.Sign((int)data.Visual.Facing) == Math.Sign(-flooredDistanceX) ? _solidObjectData.Target : null;
		
		if (clipX != 0 && Math.Sign(clipX) == Math.Sign(data.Movement.Velocity.X))
		{
			data.Movement.GroundSpeed.Value = 0f;
			data.Movement.Velocity.X = 0f;

			if (data.Movement.IsGrounded)
			{
				data.Collision.PushObjects.Add(_solidObjectData.Target.SolidBox);
			}
		}

		data.Node.Position -= new Vector2(clipX, 0f);
	}
	
	private bool CollideVertically(int clipY)
	{
		switch (clipY)
		{
			case < 0: 
				return CollideUpward(_solidObjectData.Target.SolidBox, clipY);
			
			case < 16 when _solidObjectData.Type != SolidType.Sides: 
				CollideDownward(clipY);
				return true;
			
			default: 
				ClearPush(_solidObjectData.Target);
				return true;
		}
	}
	
	private bool CollideUpward(SolidBox box, int clipY)
	{
		if (_solidObjectData.Type is SolidType.ItemBox or SolidType.Sides) return false;

		if (data.Movement.Velocity.Y == 0f && data.Movement.IsGrounded)
		{
			return CrushPlayer(clipY);
		}

		HandleUpwardCollision(box, clipY);
		return true;
	}
	
	private bool CrushPlayer(int clipY)
	{
		if (Math.Abs(clipY) < 16) return false;
		logic.Kill();
		return true;
	}
	
	private void HandleUpwardCollision(SolidBox box, int clipY)
	{
		if (data.Movement.Velocity.Y < 0f)
		{
#if S3_PHYSICS || SK_PHYSICS
			if (!data.Movement.IsGrounded)
			{
				data.Movement.GroundSpeed.Value = 0f;
			}
#endif
			data.Node.Position -= new Vector2(0, clipY);
			data.Movement.Velocity.Y = 0f;
		}

		data.Collision.TouchObjects[box] = TouchState.Bottom;
	}
	
	private void CollideDownward(int clipY)
	{
		if (data.Movement.Velocity.Y < 0f) return;
		
		float relativeX = Math.Abs(MathF.Floor(data.Node.Position.X - _solidObjectData.Position.X));
		if (relativeX > _solidObjectData.Target.SolidBox.Radius.X + _solidObjectData.ExtraSize.X) return;
		
		data.Collision.TouchObjects[_solidObjectData.Target.SolidBox] = TouchState.Top;
		AttachToObject(clipY - GripY);
	}
	
	private void CollideWithPlatformObject()
	{
		if ((int)Math.Abs(data.Node.Position.X - _solidObjectData.Position.X) > 
		    _solidObjectData.Target.SolidBox.Radius.X + _solidObjectData.ExtraSize.X)
		{
			return;
		}
			
		float clipY = MathF.Floor(_solidObjectData.Position.Y - _solidObjectData.Target.SolidBox.Radius.Y) - 
		              MathF.Floor(data.Node.Position.Y + data.Node.SolidBox.Radius.Y) - GripY;
			
		if (clipY is < -16 or >= 0) return;
		
		data.Collision.TouchObjects[_solidObjectData.Target.SolidBox] = TouchState.Top;
		AttachToObject(-((int)clipY + GripY));
	}
	
	private void AttachToObject(int distance)
	{
		if (_solidObjectData.Type is SolidType.FullReset or SolidType.TopReset)
		{
			logic.ResetState();
			logic.Action = ActionFsm.States.Default;
		}

		data.Node.Position = data.Node.Position with { Y = data.Node.Position.Y - distance - 1 };
		data.Movement.GroundSpeed.Value = data.Movement.Velocity.X;
		data.Collision.OnObject = _solidObjectData.Target;
		data.Movement.Velocity.Y = 0f;
		data.Movement.Angle = 0f;

		if (data.Movement.IsGrounded) return;
		logic.Land();
	}
}
