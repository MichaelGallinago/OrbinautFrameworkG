using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public partial class ObjectInteractivePlayer : BasicPhysicalPlayer
{
	private const int GripY = 4;
	
    public void ActSolid(BaseObject baseObject, Constants.SolidType type)
	{
		// Initialise flags for the player collision
		TouchObjects.TryAdd(baseObject, Constants.TouchState.None);
		PushObjects.Add(baseObject);
		
		if (!ObjectInteraction) return;
		if (SolidData.Radius.X <= 0 || SolidData.Radius.Y <= 0) return;
		if (baseObject.SolidData.Radius.X <= 0 || baseObject.SolidData.Radius.Y <= 0) return;
		
		var objectData = new SolidObjectData(baseObject, type);
		Vector2I extraSize = GetExtraSize();
		
		RegisterCollisionCheck();
		
		if (OnObject == baseObject)
		{
			StandingOnObject(baseObject, ref objectData, extraSize);
		}
		else if (type != Constants.SolidType.Top)
		{
			CollideWithRegularObject(baseObject, ref objectData, extraSize);
		}
		else if (Velocity.Y >= 0f)
		{
			CollideWithPlatformObject(baseObject, ref objectData, extraSize);
		}
	}
    
	private Vector2I GetExtraSize()
	{
		// Extend the radii for better & fair solid collision (if enabled)
		return SharedData.BetterSolidCollision ? new Vector2I(SolidData.Radius.X, GripY) : Vector2I.Zero;
	}
	
	private int GetSlopeOffset(short[] heightMap, SolidObjectData objectData, Vector2 scale)
	{
		// Adjust slope offset based on height map
		if (heightMap is not { Length: > 0 }) return 0;
		
		int index = Math.Clamp(
			Mathf.FloorToInt(Position.X - objectData.Position.X) * (scale.X >= 0 ? 1 : -1) + objectData.Radius.X, 
			0, heightMap.Length - 1);
			
		return (objectData.Radius.Y - heightMap[index]) * (int)scale.Y;
	}

	private void RegisterCollisionCheck()
	{
		if (SharedData.DebugCollision != 3) return;
		// TODO: debug
		/*
		var dsList = c_engine.collision.ds_solids;

		if (ds_list_find_index(dsList, this) == -1)
		{
			ds_list_add(dsList, objectPosition.X - objectRadius.X, objectPosition.Y - objectRadius.Y + slopeOffset, objectPosition.X + objectRadius.X, objectPosition.Y + objectRadius.Y + slopeOffset, this);
		}

		if (ds_list_find_index(dsList, player) == -1)
		{
			ds_list_add(dsList, position.X - radius.X, position.Y - SolidData.Radius.Y, position.X + radius.X, position.Y + SolidData.Radius.Y, player);
		}
		*/
	}

	private void StandingOnObject(BaseObject baseObject, ref SolidObjectData objectData, Vector2I extraSize)
	{
		TouchObjects[baseObject] = Constants.TouchState.Up;
		
		// Adjust player's position
		int slopeOffset = GetSlopeOffset(baseObject.SolidData.HeightMap, objectData, baseObject.Scale);
		Position = objectData.Position + new Vector2(
			Position.X - baseObject.SolidData.Offset.X - baseObject.PreviousPosition.X, 
			slopeOffset - objectData.Radius.Y - SolidData.Radius.Y - 1);
			
		float relativeX = Math.Abs(MathF.Floor(Position.X - objectData.Position.X)) - objectData.Radius.X;
		if (objectData.Type == Constants.SolidType.Top ? relativeX <= extraSize.X : relativeX < 0f) return;
			
		// Reset touch flags and player's on-object status if they are out of bounds
		TouchObjects[baseObject] = Constants.TouchState.None;
		OnObject = null;
	}

	private void CollideWithRegularObject(BaseObject baseObject, ref SolidObjectData objectData, Vector2I extraSize)
	{
		// Calculate distances for collision detection
		Vector2I combinedSize = objectData.Radius + SolidData.Radius;
		combinedSize.X++;
		
		Vector2I distance = (Vector2I)(Position - objectData.Position) + combinedSize;
		distance.Y += GripY - GetSlopeOffset(baseObject.SolidData.HeightMap, objectData, baseObject.Scale);
		
		// Check if player is out of bounds
		if (distance.X < 0 || distance.X > combinedSize.X * 2 || 
		    distance.Y < 0 || distance.Y > combinedSize.Y * 2 + extraSize.Y)
		{
			ClearPush();
			return;
		}
		
		var flooredPlayerPosition = (Vector2I)Position;
		var flooredObjectPosition = (Vector2I)objectData.Position;
		
		Vector2I clip = distance - new Vector2I(
			flooredPlayerPosition.X < flooredObjectPosition.X ? 0 : combinedSize.X * 2,
			flooredPlayerPosition.Y < flooredObjectPosition.Y ? 0 : combinedSize.Y * 2 + GripY);

		Vector2I clipAbsolute = clip.Abs();
		if (clipAbsolute.X >= clipAbsolute.Y || SharedData.PlayerPhysics == PhysicsTypes.SK && clipAbsolute.Y <= 4)
		{
			CollideVertical(baseObject, ref objectData, extraSize.X, clip.Y);
			return;
		}
		
		if (SharedData.PlayerPhysics != PhysicsTypes.SK && clipAbsolute.Y <= 4)
		{
			ClearPush();
			return;
		}
		
		// Update player's pushing status if grounded
		SetPushAnimationBy = IsGrounded && 
		    Math.Sign((int)Facing) == Math.Sign(flooredObjectPosition.X - flooredPlayerPosition.X) ?
			baseObject : null;
		
		TouchObjects[baseObject] = flooredPlayerPosition.X < flooredObjectPosition.X ? 
			Constants.TouchState.Left : Constants.TouchState.Right;
		
		if (clip.X != 0 && Math.Sign(clip.X) == Math.Sign(Velocity.X))
		{
			GroundSpeed = 0f;
			Velocity.X = 0f;

			if (IsGrounded)
			{
				PushObjects.Add(baseObject);
			}
		}

		Position -= new Vector2(clip.X, 0f);
	}

	private void CollideVertical(BaseObject baseObject, ref SolidObjectData objectData, int extraSizeX, int clipY)
	{
		switch (clipY)
		{
			case < 0 when objectData.Type != Constants.SolidType.ItemBox: CollideFromBelow(baseObject, clipY); break;
			case >= 0 and < 16: CollideDownward(baseObject, ref objectData, extraSizeX, clipY); break;
			default: ClearPush(); break;
		}
	}

	private void CollideFromBelow(BaseObject baseObject, int clipY)
	{
		switch (Velocity.Y)
		{
			case 0: CrushPlayer(MathF.Abs(clipY) < 16); break;
			case < 0: HandleUpwardCollision(baseObject, clipY); break;
		}
	}

	private void CrushPlayer(bool condition)
	{
		if (!IsGrounded || !condition) return;
		Kill();
	}

	private void HandleUpwardCollision(BaseObject baseObject, int clipY)
	{
		if (SharedData.PlayerPhysics >= PhysicsTypes.S3 && !IsGrounded)
		{
			GroundSpeed = 0f;
		}

		Position = new Vector2(Position.X, Position.Y - clipY);
		Velocity.Y = 0f;
		TouchObjects[baseObject] = Constants.TouchState.Down;
	}

	private void CollideDownward(BaseObject baseObject, ref SolidObjectData objectData, int extraSizeX, int clipY)
	{
		if (Velocity.Y < 0) return;
		
		float relX = MathF.Floor(Position.X - objectData.Position.X) + objectData.Radius.X;
		if (relX < -extraSizeX || relX > objectData.Radius.X * 2 + extraSizeX) return;
		
		TouchObjects[baseObject] = Constants.TouchState.Up;
		LandOnSolid(baseObject, objectData.Type, Mathf.FloorToInt(clipY - GripY));
	}

	private void CollideWithPlatformObject(BaseObject baseObject, ref SolidObjectData objectData, Vector2I extraSize)
	{
		if (Math.Abs(MathF.Floor(Position.X - objectData.Position.X)) > objectData.Radius.X + extraSize.X) return;
			
		float yClip = MathF.Floor(objectData.Position.Y - objectData.Radius.Y) - 
		              MathF.Floor(Position.Y + SolidData.Radius.Y) - GripY;
			
		if (yClip is < -16 or >= 0) return;
			
		TouchObjects[baseObject] = Constants.TouchState.Up;
		LandOnSolid(baseObject, objectData.Type, -((int)yClip + GripY));
	}
}
