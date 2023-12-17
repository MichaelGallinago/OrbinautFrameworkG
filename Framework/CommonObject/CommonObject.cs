using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework.CommonObject;

public abstract partial class CommonObject : Node2D
{
	public enum BehaviourType : byte
	{
		Active, Reset, Pause, Delete, Unique
	}
    
	[Export] public BehaviourType Behaviour { get; set; }
    
	public static List<CommonObject> Objects { get; }
	public ObjectRespawnData RespawnData { get; }
	public SolidData SolidData { get; set; }
	public Animations.AnimatedSprite Sprite { get; set; }
	public Vector2 PreviousPosition { get; set; }

	public InteractData InteractData;
 
	static CommonObject()
	{
		Objects = [];
	}

	protected CommonObject()
	{
		RespawnData = new ObjectRespawnData(Position, Scale, Visible, ZIndex);
		InteractData = new InteractData();
		SolidData = new SolidData();
	}

	public override void _EnterTree()
	{
		Objects.Add(this);

		FrameworkData.CurrentScene.PreUpdate += PreUpdate;
		FrameworkData.CurrentScene.EarlyUpdate += EarlyUpdate;
		FrameworkData.CurrentScene.Update += Update;
		FrameworkData.CurrentScene.LateUpdate += LateUpdate;
	}

	public override void _ExitTree()
	{
		Objects.Remove(this);
        
		FrameworkData.CurrentScene.PreUpdate -= PreUpdate;
		FrameworkData.CurrentScene.EarlyUpdate -= EarlyUpdate;
		FrameworkData.CurrentScene.Update -= Update;
		FrameworkData.CurrentScene.LateUpdate -= LateUpdate;
	}

	private void PreUpdate(double processSpeed)
	{
		PreviousPosition = Position;
	}
	
	protected virtual void EarlyUpdate(double processSpeed) {}
	protected virtual void Update(double processSpeed) {}
	protected virtual void LateUpdate(double processSpeed) {}
	protected virtual void Initialize() {}

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

	public void SetHitboxExtra(Vector2I radius, Vector2I offset = default)
	{
		InteractData.RadiusExtra = radius;
		InteractData.OffsetExtra = offset;
	}

	public void ActSolid(Player player, Constants.SolidType type)
	{
		// The following is long and replicates the method of colliding
		// with an object from the original games
		
		// Initialise touch flags for the player collision
		SolidData.TouchStates[player.Id] = Constants.TouchState.None;
		
		// Check if player properties are valid
		Vector2I playerRadius = player.SolidData.Radius;
		if (playerRadius.X <= 0 || playerRadius.Y <= 0 || !player.ObjectInteraction) return;
		
		// Check if object radius are valid
		Vector2I objectRadius = SolidData.Radius;
		if (objectRadius.X <= 0 ||objectRadius.Y <= 0) return;
		
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
		if (objHeightMap.Length > 0)
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
			SolidData.TouchStates[player.Id] = Constants.TouchState.Up;
			
			// Adjust player's position
			player.Position = new Vector2(
				playerPosition.X + objectPosition.X - objectPreviousX, 
				objectPosition.Y - objectRadius.Y + slopeOffset - playerRadius.Y - 1);
			playerPosition.X = player.Position.X;
			
			if (type == Constants.SolidType.Top) return;
			
			if (Math.Abs(MathF.Floor(playerPosition.X - objectPosition.X)) < combinedSize.X) return;
			
			// Reset touch flags and player's on-object status if they are out of bounds
			SolidData.TouchStates[player.Id] = Constants.TouchState.None;
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
				ClearPush(player);
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
								SolidData.TouchStates[player.Id] = Constants.TouchState.Down;
								break;
						}
						break;
					// Handle downward (top) collision
					case >= 0 and < 16:
						if (player.Speed.Y < 0) return;
						
						float relX = MathF.Floor(playerPosition.X - objectPosition.X) + objectRadius.X;
						if (relX >= 0 - extraSize.X && relX <= objectRadius.X * 2 + extraSize.X)
						{
							SolidData.TouchStates[player.Id] = Constants.TouchState.Up;
							
							// Attach player to the object
							LandOnSolid(player, this, type, Mathf.FloorToInt(clip.Y - gripY));
						}
						break;
					// If failed to collide vertically, clear push flag
					default:
						ClearPush(player);
						break;
				}
				
				// Do not perform horizontal collision
				return;
			}
				
			// HORIZONTAL COLLISION
			if (!(SharedData.PlayerPhysics == Player.PhysicsTypes.SK || Math.Abs(clip.Y) > 4))
			{
				ClearPush(player);
				return;
			}
			
			// Update player's pushing status if grounded
			player.PushingObject = player.IsGrounded && 
			    Math.Sign((sbyte)player.Facing) == Math.Sign(integerObjectPosition.X - integerPlayerPosition.X) ? this : null;
			
			SolidData.TouchStates[player.Id] = integerPlayerPosition.X < integerObjectPosition.X ? 
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
			
			SolidData.TouchStates[player.Id] = Constants.TouchState.Up;
			
			// Attach player to the object
			LandOnSolid(player, this, type, -((int)yClip + gripY));
		}
	}
    
	private static void LandOnSolid(Player player, CommonObject targetObject, Constants.SolidType type, int distance)
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
    
	private void ClearPush(Player player)
	{
		if (player.PushingObject != this) return;
		if (player.Animation != Player.Animations.Spin)
		{
			player.Animation = Player.Animations.Move;
		}
				
		player.PushingObject = null;
	}
}