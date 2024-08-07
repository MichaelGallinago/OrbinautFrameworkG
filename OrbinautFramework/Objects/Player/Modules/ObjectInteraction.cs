using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Physics;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct ObjectInteraction
{
	private const int GripY = 4;
	
	private SolidObjectData _data;
	
	public void ClearPush(OrbinautData target)
	{
		if (SetPushAnimationBy != target) return;
		SetPushAnimationBy = null;
		
		if (Animation is not (Animations.Spin or Animations.SpinDash))
		{
			Animation = Animations.Move;
		}
	}

	public void ActSolid(OrbinautData orbinautData, SolidType type, bool isFullRoutine = true)
	{
		// Initialise flags for the player collision
		TouchObjects.TryAdd(orbinautData, TouchState.None);
		PushObjects.Add(orbinautData);
		
		if (!IsObjectInteractionEnabled) return;
		if (SolidBox.Radius.X <= 0 || SolidBox.Radius.Y <= 0) return;
		if (orbinautData.SolidBox.Radius.X <= 0 || orbinautData.SolidBox.Radius.Y <= 0) return;
		
		_data = new SolidObjectData(orbinautData, type, GetExtraSize());
		int slopeOffset = CalculateSlopeOffset();
		
		RegisterCollisionCheck();
		
		if (OnObject == orbinautData)
		{
			StandingOnObject(slopeOffset);
			return;
		}
		
		if (!isFullRoutine) return;
		
		if (type != SolidType.Top)
		{
			CollideWithRegularObject(slopeOffset);
		}
		else if (Velocity.Y >= 0f)
		{
			CollideWithPlatformObject();
		}
	}
	
	public bool CheckSolidCollision(SolidBox solidBox, CollisionSensor type)
	{
		if (!IsObjectInteractionEnabled) return false;

		// No solid collision data, exit collision check
		if (!TouchObjects.TryGetValue(solidBox, out TouchState touchState)) return false;
		
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
	
	private Vector2I GetExtraSize()
	{
		// Extend the radiuses for better & fair solid collision (if enabled)
		return SharedData.BetterSolidCollision ? new Vector2I(SolidBox.Radius.X, GripY) : Vector2I.Zero;
	}
	
	private int CalculateSlopeOffset()
	{
		// Adjust slope offset based on height map
		short[] heightMap = _data.Target.SolidBox.HeightMap;
		
		if (heightMap is not { Length: > 0 }) return 0;

		int distance = Mathf.FloorToInt(Position.X - _data.Position.X) * (_data.Target.Scale.X >= 0 ? 1 : -1);
		int index = Math.Clamp(distance + _data.Target.SolidBox.Radius.X, 0, heightMap.Length - 1);
		
		return (_data.Target.SolidBox.Radius.Y - heightMap[index]) * (int)_data.Target.Scale.Y;
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
		TouchObjects[_data.Target] = TouchState.Top;
		
		// Adjust player's position
		Position = _data.Position + new Vector2(
			Position.X - _data.Target.PreviousPosition.X, 
			slopeOffset - _data.Target.SolidBox.Radius.Y - SolidBox.Radius.Y - 1);

		float relativeX = Math.Abs(MathF.Floor(Position.X - _data.Position.X)) - _data.Target.SolidBox.Radius.X;

		if (_data.Type == SolidType.Top ? relativeX <= _data.ExtraSize.X : relativeX < SolidBox.Radius.X + 1)
		{
			return;
		}
			
		// Reset touch flags and player's on-object status if they are out of bounds
		TouchObjects[_data.Target] = TouchState.None;
		OnObject = null;
	}
	
	private void CollideWithRegularObject(int slopeOffset)
	{
		// Calculate distances for collision detection
		Vector2I combinedSize = _data.Target.SolidBox.Radius + SolidBox.Radius;
		combinedSize.X++;
		
		Vector2I distance = (Vector2I)(Position - _data.Position) + combinedSize;
		distance.Y += GripY - slopeOffset;
		
		// Check if player is out of bounds
		if (distance.X < 0 || distance.X > combinedSize.X * 2 || 
		    distance.Y < 0 || distance.Y > combinedSize.Y * 2 + _data.ExtraSize.Y)
		{
			ClearPush(_data.Target);
			return;
		}

		Vector2I flooredDistance = (Vector2I)Position - (Vector2I)_data.Position;
		
		Vector2I clip = distance - new Vector2I(
			flooredDistance.X < 0 ? 0 : combinedSize.X * 2, 
			flooredDistance.Y < 0 ? 0 : combinedSize.Y * 2 + GripY);

		Vector2I clipAbsolute = clip.Abs();
		bool isLimitedClip = clipAbsolute.Y <= 4;

		bool isS3Physics = SharedData.PhysicsType >= PhysicsTypes.S3;
		if (clipAbsolute.X >= clipAbsolute.Y || isS3Physics && isLimitedClip)
		{
			if (CollideVertically(clip.Y, isS3Physics)) return;
		}

		CollideHorizontally(isLimitedClip, flooredDistance.X, clip.X, isS3Physics);
	}

	private void CollideHorizontally(bool isLimitedClip, int flooredDistanceX, int clipX, bool isS3Physics)
	{
		if (!isS3Physics && isLimitedClip)
		{
			ClearPush(_data.Target); 
			return;
		}
		
		TouchObjects[_data.Target] = flooredDistanceX < 0 ? TouchState.Left : TouchState.Right;

		UpdatePushingStatus(clipX, flooredDistanceX);
	}
	
	private void UpdatePushingStatus(int clipX, int flooredDistanceX)
	{
		SetPushAnimationBy = IsGrounded && Math.Sign((int)Facing) == Math.Sign(-flooredDistanceX) ? _data.Target : null;
		
		if (clipX != 0 && Math.Sign(clipX) == Math.Sign(Velocity.X))
		{
			GroundSpeed.Value = 0f;
			Velocity.X = 0f;

			if (IsGrounded)
			{
				PushObjects.Add(_data.Target);
			}
		}

		Position -= new Vector2(clipX, 0f);
	}
	
	private bool CollideVertically(int clipY, bool isS3Physics)
	{
		switch (clipY)
		{
			case < 0: 
				return CollideUpward(_data.Target, clipY, isS3Physics);
			
			case < 16 when _data.Type != SolidType.Sides: 
				CollideDownward(clipY);
				return true;
			
			default: 
				ClearPush(_data.Target);
				return true;
		}
	}
	
	private bool CollideUpward(OrbinautData orbinautData, int clipY, bool isS3Physics)
	{
		if (_data.Type is SolidType.ItemBox or SolidType.Sides) return false;

		if (Velocity.Y == 0f && IsGrounded)
		{
			return CrushPlayer(clipY);
		}

		HandleUpwardCollision(orbinautData, clipY, isS3Physics);
		return true;
	}
	
	private bool CrushPlayer(int clipY)
	{
		if (Math.Abs(clipY) < 16) return false;
		Kill();
		return true;
	}
	
	private void HandleUpwardCollision(OrbinautData orbinautData, int clipY, bool isS3Physics)
	{
		if (Velocity.Y < 0f)
		{
			if (isS3Physics && !IsGrounded)
			{
				GroundSpeed.Value = 0f;
			}

			Position = new Vector2(Position.X, Position.Y - clipY);
			Velocity.Y = 0f;
		}

		TouchObjects[orbinautData] = TouchState.Bottom;
	}
	
	private void CollideDownward(int clipY)
	{
		if (Velocity.Y < 0f) return;
		
		float relativeX = Math.Abs(MathF.Floor(Position.X - _data.Position.X));
		if (relativeX > _data.Target.SolidBox.Radius.X + _data.ExtraSize.X) return;
		
		TouchObjects[_data.Target] = TouchState.Top;
		AttachToObject(clipY - GripY);
	}
	
	private void CollideWithPlatformObject()
	{
		if (Math.Abs((int)(Position.X - _data.Position.X)) > _data.Target.SolidBox.Radius.X + _data.ExtraSize.X)
		{
			return;
		}
			
		float clipY = MathF.Floor(_data.Position.Y - _data.Target.SolidBox.Radius.Y) - 
		              MathF.Floor(Position.Y + SolidBox.Radius.Y) - GripY;
			
		if (clipY is < -16 or >= 0) return;
			
		TouchObjects[_data.Target] = TouchState.Top;
		AttachToObject(-((int)clipY + GripY));
	}
	
	private void AttachToObject(int distance)
	{
		if (_data.Type is SolidType.FullReset or SolidType.TopReset)
		{
			ResetState();
		}

		Position = Position with { Y = Position.Y - distance - 1 };
		GroundSpeed.Value = Velocity.X;
		OnObject = _data.Target;
		Velocity.Y = 0f;
		Angle = 0f;

		if (IsGrounded) return;
		Land();
	}
}
