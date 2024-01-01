using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.CommonObject;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class BaseObject : Node2D
{
	public enum BehaviourType : byte
	{
		NoBounds, Reset, Pause, Delete, Unique
	}
    
	[Export] public BehaviourType Behaviour { get; set; }
    
	public static List<BaseObject> Objects { get; }
	public ObjectRespawnData RespawnData { get; }
	public SolidData SolidData { get; set; }
	public Vector2 PreviousPosition { get; set; }

	public InteractData InteractData;
 
	static BaseObject()
	{
		Objects = [];
	}

	protected BaseObject()
	{
		RespawnData = new ObjectRespawnData(Position, Scale, Visible, ZIndex);
		InteractData = new InteractData();
		SolidData = new SolidData();
	}
	
	public virtual void Init() {}

	public override void _EnterTree()
	{
		Objects.Add(this);
	}

	public override void _ExitTree() => Objects.Remove(this);

	public void SetBehaviour(BehaviourType behaviour)
	{
		if (Behaviour == BehaviourType.Delete) return;
		Behaviour = behaviour;
	}

	public void ResetZIndex() => ZIndex = RespawnData.ZIndex;
    
	public void SetSolid(Vector2I radius, Vector2I offset = new())
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

	public void SetActivity(bool isActive)
	{
		SetProcess(isActive);
		Visible = isActive;
	}

	public void ActSolid(Player player, Constants.SolidType type)
	{
		// The following is long and replicates the method of colliding
		// with an object from the original games
		
		// Initialise touch flags for the player collision
		player.TouchObjects.Add(this, Constants.TouchState.None);
		
		// Check if player properties are valid
		Vector2I playerRadius = player.SolidData.Radius;
		if (playerRadius.X <= 0 || playerRadius.Y <= 0 || !player.ObjectInteraction) return;
		
		// Check if object radius are valid
		Vector2I objectRadius = SolidData.Radius;
		if (objectRadius.X <= 0 || objectRadius.Y <= 0) return;
		
		Vector2 playerPosition = player.Position;
		var integerPlayerPosition = (Vector2I)playerPosition;
		
		float objectPreviousX = SolidData.Offset.X + PreviousPosition.X;
		Vector2 objectPosition = SolidData.Offset + Position;
		short[] objHeightMap = SolidData.HeightMap;
		var integerObjectPosition = (Vector2I)objectPosition;
		
		// Combined width and height for collision calculations
		Vector2I combinedSize = objectRadius + playerRadius;
		combinedSize.X++;
		
		var slopeOffset = 0;
		const int gripY = 4;
		var extraSize = new Vector2I();
		
		// Adjust slope offset based on height map
		if (objHeightMap is { Length: > 0 })
		{
			int index = Math.Clamp(
				Mathf.FloorToInt(playerPosition.X - objectPosition.X) * (Scale.X >= 0 ? 1 : -1) + objectRadius.X, 
				0, objHeightMap.Length - 1);
			
			slopeOffset = (objectRadius.Y - objHeightMap[index]) * (int)Scale.Y;
		}
		
		// Extend the radiuses for better & fair solid collision (if enabled)
		if (SharedData.BetterSolidCollision)
		{
			extraSize = new Vector2I(playerRadius.X, gripY);
		}
		
		// Register collision check if debugging
		if (SharedData.DebugCollision == 3)
		{
			// TODO: debug
			/*
			var dsList = c_engine.collision.ds_solids;
			
			if (ds_list_find_index(dsList, this) == -1)
			{
				ds_list_add(dsList, objectPosition.X - objectRadius.X, objectPosition.Y - objectRadius.Y + slopeOffset, objectPosition.X + objectRadius.X, objectPosition.Y + objectRadius.Y + slopeOffset, this);
			}
			
			if (ds_list_find_index(dsList, player) == -1)
			{
				ds_list_add(dsList, position.X - radius.X, position.Y - playerRadius.Y, position.X + radius.X, position.Y + playerRadius.Y, player);
			}
			*/
		}
		
		// Is player standing on this object?
		if (player.OnObject == this)
		{
			player.TouchObjects[this] = Constants.TouchState.Up;
			
			// Adjust player's position
			player.Position = objectPosition + new Vector2(
				playerPosition.X - objectPreviousX, 
				slopeOffset - objectRadius.Y - playerRadius.Y - 1);
			playerPosition.X = player.Position.X;
			
			float relativeX = Math.Abs(MathF.Floor(playerPosition.X - objectPosition.X)) - objectRadius.X;
			if (type == Constants.SolidType.Top ? relativeX <= extraSize.X : relativeX < 0f) return;
			
			// Reset touch flags and player's on-object status if they are out of bounds
			player.TouchObjects[this] = Constants.TouchState.None;
			player.OnObject = null;
		}
		
		// Handle collision with a regular object
		else if (type != Constants.SolidType.Top)
		{
			// Calculate distances for collision detection
			Vector2I distance = (Vector2I)(playerPosition - objectPosition) + combinedSize;
			distance.Y += gripY - slopeOffset;
			
			// Check if player is out of bounds
			if (distance.X < 0 || distance.X > combinedSize.X * 2 || 
			    distance.Y < 0 || distance.Y > combinedSize.Y * 2 + extraSize.Y)
			{
				player.ClearPush();
				return;
			}
			
			Vector2I clip = distance - new Vector2I(
				integerPlayerPosition.X < integerObjectPosition.X ? 0 : combinedSize.X * 2,
				integerPlayerPosition.Y < integerObjectPosition.Y ? 0 : combinedSize.Y * 2 + gripY);
			
			bool vCollision = Math.Abs(clip.X) >= Math.Abs(clip.Y) || 
			    SharedData.PlayerPhysics == Player.PhysicsTypes.SK && Math.Abs(clip.Y) <= 4;
				
			// VERTICAL COLLISION
			if (vCollision)
			{
				switch (clip.Y)
				{
					// Try to collide from below
					case < 0 when type != Constants.SolidType.ItemBox:
						switch (player.Speed.Y)
						{
							// Crush the player
							case 0 when player.IsGrounded:
								if (MathF.Abs(clip.Y) < 16) break;
								player.Kill();
								break;
							// Handle upward (bottom) collision
							case < 0:
								if (SharedData.PlayerPhysics >= Player.PhysicsTypes.S3 && !player.IsGrounded)
								{
									player.GroundSpeed = 0;
								}
								
								player.Position = new Vector2(player.Position.X, player.Position.Y - clip.Y);
								player.Speed = new Vector2(player.Speed.X, 0);
						
								// Set collision flag
								player.TouchObjects[this] = Constants.TouchState.Down;
								break;
						}
						break;
					// Handle downward (top) collision
					case >= 0 and < 16:
						if (player.Speed.Y < 0) return;
						
						float relX = MathF.Floor(playerPosition.X - objectPosition.X) + objectRadius.X;
						if (relX >= 0 - extraSize.X && relX <= objectRadius.X * 2 + extraSize.X)
						{
							player.TouchObjects[this] = Constants.TouchState.Up;
							
							// Attach player to the object
							LandOnSolid(player, this, type, Mathf.FloorToInt(clip.Y - gripY));
						}
						break;
					// If failed to collide vertically, clear push flag
					default:
						player.ClearPush();
						break;
				}
				
				// Do not perform horizontal collision
				return;
			}
				
			// HORIZONTAL COLLISION
			if (!(SharedData.PlayerPhysics == Player.PhysicsTypes.SK || Math.Abs(clip.Y) > 4))
			{
				player.ClearPush();
				return;
			}
			
			// Update player's pushing status if grounded
			player.PushingObject = player.IsGrounded && 
			    Math.Sign((int)player.Facing) == Math.Sign(integerObjectPosition.X - integerPlayerPosition.X) ? 
				this : null;
			
			player.TouchObjects[this] = integerPlayerPosition.X < integerObjectPosition.X ? 
				Constants.TouchState.Left : Constants.TouchState.Right;
			
			if (clip.X != 0 && Math.Sign(clip.X) == Math.Sign(player.Speed.X))
			{
				player.GroundSpeed = 0;
				player.Speed = new Vector2(0, player.Speed.Y);
			}

			player.Position = new Vector2(player.Position.X - clip.X, player.Position.Y);
		}
		
		// Handle collision with a platform object
		else if (player.Speed.Y >= 0f)
		{
			if (Math.Abs(MathF.Floor(playerPosition.X - objectPosition.X)) > objectRadius.X + extraSize.X) return;
			
			float yClip = MathF.Floor(objectPosition.Y - objectRadius.Y) - 
			              MathF.Floor(playerPosition.Y + playerRadius.Y) - gripY;
			
			if (yClip is < -16 or >= 0) return;
			
			player.TouchObjects[this] = Constants.TouchState.Up;
			
			// Attach player to the object
			LandOnSolid(player, this, type, -((int)yClip + gripY));
		}
	}
	
	public bool CheckCollision(BaseObject target, Constants.CollisionSensor type)
	{
		if (target is Player { ObjectInteraction: false }) return false;

		return type switch
		{
			Constants.CollisionSensor.Hitbox => CheckHitboxCollision(target, type),
			Constants.CollisionSensor.HitboxExtra => CheckHitboxCollision(target, type),
			_ => CheckSolidCollision(target, type)
		};
	}
	
	private bool CheckHitboxCollision(BaseObject target, Constants.CollisionSensor type)
	{
		if (!InteractData.IsInteract || !target.InteractData.IsInteract) return false;
		var debugColor = new Color();

		var targetOffset = new Vector2I();
		var targetRadius = new Vector2I();
		if (type == Constants.CollisionSensor.HitboxExtra)
		{
			targetRadius = target.InteractData.RadiusExtra;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0)
			{
				type = Constants.CollisionSensor.Hitbox;	
			}	
			else
			{
				targetOffset = target.InteractData.OffsetExtra;
				debugColor = Color.Color8(0, 0, 220);
			}
		}

		if (type == Constants.CollisionSensor.Hitbox)
		{
			targetRadius = target.InteractData.Radius;
			if (targetRadius.X <= 0 || targetRadius.Y <= 0) return false;
			
			targetOffset = target.InteractData.Offset;
			debugColor = Color.Color8(255, 0, 220);
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
				
			if ds_list_find_index(_ds_list, _target.id) == -1 || ds_list_find_index(_ds_list, _target_col) == -1
			{
				ds_list_add(_ds_list, _target_l, _target_t, _target_r, _target_b, _target_col, _target.id);
			}
				
			if ds_list_find_index(_ds_list, id) == -1 || ds_list_find_index(_ds_list, _target_col) == -1
			{
				ds_list_add(_ds_list, _this_l, _this_t, _this_r, _this_b, _target_col, id);
			}
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

	private bool CheckSolidCollision(BaseObject target, Constants.CollisionSensor type)
	{
		if (target is not Player player) return false;

		// No solid collision data, exit collision check
		if (!player.TouchObjects.TryGetValue(this, out Constants.TouchState touchState)) return false;
			
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
			Constants.CollisionSensor.SolidU => touchState == Constants.TouchState.Up,
			Constants.CollisionSensor.SolidD => touchState == Constants.TouchState.Down,
			Constants.CollisionSensor.SolidL => touchState == Constants.TouchState.Left,
			Constants.CollisionSensor.SolidR => touchState == Constants.TouchState.Right,
			Constants.CollisionSensor.SolidAny => touchState != Constants.TouchState.None,
			Constants.CollisionSensor.Hitbox => false,
			Constants.CollisionSensor.HitboxExtra => false,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}
    
	private static void LandOnSolid(Player player, BaseObject targetObject, Constants.SolidType type, int distance)
	{
		if (type is Constants.SolidType.AllReset or Constants.SolidType.TopReset)
		{
			player.ResetState();
		}
				
		player.Position = new Vector2(player.Position.X, player.Position.Y - distance + 1);
				
		player.GroundSpeed = player.Speed.X;
		player.Speed = new Vector2(player.Speed.X, 0);
		player.Angle = 360f;
				
		player.OnObject = targetObject;

		if (player.IsGrounded) return;
		player.IsGrounded = true;

		player.Land();
	}
}