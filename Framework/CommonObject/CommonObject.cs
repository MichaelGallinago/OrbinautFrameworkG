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
	public InteractData InteractData { get; }
	public SolidData SolidData { get; set; }
	public Animations.AnimatedSprite Sprite { get; set; }
 
	static CommonObject()
	{
		Objects = new List<CommonObject>();
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
        
		FrameworkData.CurrentScene.EarlyUpdate += EarlyUpdate;
		FrameworkData.CurrentScene.Update += Update;
		FrameworkData.CurrentScene.LateUpdate += LateUpdate;
	}

	public override void _ExitTree()
	{
		Objects.Remove(this);
        
		FrameworkData.CurrentScene.EarlyUpdate -= EarlyUpdate;
		FrameworkData.CurrentScene.Update -= Update;
		FrameworkData.CurrentScene.LateUpdate -= LateUpdate;
	}

	protected virtual void EarlyUpdate(double processSpeed) {}
	protected virtual void Update(double processSpeed) {}
	protected virtual void LateUpdate(double processSpeed) {}

	public void SetBehaviour(BehaviourType behaviour)
	{
		if (Behaviour == BehaviourType.Delete) return;
		Behaviour = behaviour;
	}

	public void ResetZIndex()
	{
		ZIndex = RespawnData.ZIndex;
	}
    
	public void SetSolid(Vector2I radius, Vector2I offset = new())
	{
		SolidData.Radius = radius;
		SolidData.Offset = offset;
		SolidData.HeightMap = null;
	}

	public void ActSolid(OrbinautFramework3.Objects.Player.Player player, Constants.SolidType type)
	{
		// The following is long and replicates the method of colliding
		// with an object from the original games

		// Get player ID
		int pid = player.Id;
		
		// Clear collision flag
		SolidData.TouchStates[pid] = Constants.TouchState.None;
		
		// Exit if can't collide
		if (player.SolidData.Radius.X <= 0 || player.SolidData.Radius.Y <= 0 || !player.ObjectInteraction)
		{
			return;
		}
		
		if (SolidData.Radius.X <= 0 || SolidData.Radius.Y <= 0)
		{
			return;
		}
		
		// Get player data
		float px = player.Position.X;
		float py = player.Position.Y;
		int pw = player.SolidData.Radius.X;
		int ph = player.SolidData.Radius.Y;
		
		float pxf = Mathf.Floor(px);
		float pyf = Mathf.Floor(py);
		
		// Get object data
		int objXPrev = SolidData.Offset.X + xprevious;
		float objX = SolidData.Offset.X + Position.X;
		float objY = SolidData.Offset.Y + Position.Y;
		int objW = SolidData.Radius.X;
		int objH = SolidData.Radius.Y;
		short[] objHmap = SolidData.HeightMap;
		
		float objXf = Mathf.Floor(objX);
		float objYf = Mathf.Floor(objY);
		
		int combinedWidth  = objW + player.SolidData.Radius.X + 1;
		int combinedHeight = objH + player.SolidData.Radius.Y;
		
		var slopeOffset = 0f;
		var gripY = 4;
		var extX = 0;
		var extY = 0;
		
		// Calculate offset for a sloped object
		if (objHmap.Length > 0)
		{
			int index;
			
			if (Scale.X >= 0)
			{
				index = Mathf.FloorToInt(px - objX) + objW;
			}
			else
			{
				index = Mathf.FloorToInt(objX - px) + objW;
			}	
			
			index = Mathf.Clamp(index, 0, objHmap.Length - 1);
			
			slopeOffset = (objH - objHmap[index]) * Scale.Y;	
		}
		else
		{
			slopeOffset = 0;
		}
		
		// Extend collision box
		if (SharedData.BetterSolidCollision)
		{
			extX = pw;
			extY = gripY;
		}
		
		// Add collision check to the debug list
		if (SharedData.DebugCollision == 3)
		{
			var dsList = c_engine.collision.ds_solids;
			
			if (ds_list_find_index(dsList, this) == -1)
			{
				ds_list_add(dsList, objX - objW, objY - objH + slopeOffset, objX + objW, objY + objH + slopeOffset, this);
			}
			
			if (ds_list_find_index(dsList, player) == -1)
			{
				ds_list_add(dsList, px - pw, py - ph, px + pw, py + ph, player);
			}
		}
		
		// Is player standing on this object?
		if (player.OnObject == this)
		{	
			// Set collision flag
			SolidData.TouchStates[pid] = Constants.TouchState.Up;
			
			// Move player with the object
			player.Position.X += objX - objXPrev;
			player.Position.Y = objY - objH + slopeOffset - ph - 1;
			
			px = player.Position.X;
			
			// Is player still within the object?
			if (type != Constants.SolidType.Top)
			{
				float relX = Mathf.Floor(px - objX) + combinedWidth;
				if (relX > 0 && relX < combinedWidth * 2)
				{
					return;
				}
			}
			else
			{
				float relX = Mathf.Floor(px - objX) + objW;
				if (relX >= 0 - extX && relX <= objW * 2 + extX)
				{
					return;
				}
			}
			
			// If not, clear collision flag
			SolidData.TouchStates[pid] = Constants.TouchState.None;
			
			// Clear player's flag
			player.OnObject = null;
		}
		
		// Is player trying to collide with non-platform object?
		else if (_type != SOBJ_TOP)
		{
			// Is player within the object area?
			float xDist = Mathf.Floor(px - objX) + combinedWidth;
			float yDist = Mathf.Floor(py - objY) + combinedHeight - slopeOffset + gripY;
			
			// If not, clear push flag and return
			if (xDist < 0 || xDist > combinedWidth  * 2 
			              || yDist < 0 || yDist >= combinedHeight * 2 + extY)
			{
				obj_act_solid_clear_push(player);
				return;
			}
			
			// Calculate clip distance
			float xClip = pxf < objXf ? xDist : xDist - combinedWidth  * 2;
			float yClip = pyf < objYf ? yDist : yDist - combinedHeight * 2 - gripY;
			
			// Define if player should collide vertically
			var vCollision = false;
			
			if (_type != SOBJ_SIDES)
			{
				if (Mathf.Abs(xClip) >= Mathf.Abs(yClip))
				{
					vCollision = true;
				}
				
				if (SharedData.player_physics >= PHYSICS_S3 and yClip <= 4)
				{
					vCollision = true;
				}
			}
				
			// Try to perform vertical collision
			if (vCollision)
			{	
				// Try to collide from below
				if (yClip < 0 && _type != SOBJ_ITEMBOX)
				{
					// If player is standing on the ground, kill them
					if (player.ysp == 0 && player.IsGrounded)
					{
						if (Mathf.Abs(xClip) >= 16)
						{
							player_kill(player);
						}
					}
					
					// Else just clip player out
					else if (player.ysp < 0)
					{
						if (SharedData.player_physics >= PHYSICS_S3 && !player.IsGrounded)
						{
							player.gsp = 0;
						}
								
						player.y -= yClip;
						player.ysp = 0;
						
						// Set collision flag
						SolidData.touch_flags[pid] = 2;
					}
				}
				
				// Try to collide from above
				else if (yClip >= 0 && yClip < 16)
				{
					if (player.ysp < 0)
					{
						return;
					}
					
					// If player is within the object and moving down, let them land
					float relX = Mathf.Floor(px - objX) + objW;
					if (relX >= 0 - extX && relX <= objW * 2 + extX)
					{
						obj_act_solid_land(player, id, _type, yClip - gripY);
						
						// Set collision flag
						SolidData.touch_flags[pid] = 1;
					}
				}
					
				// If failed to collide vertically, clear push flag
				else
				{
					obj_act_solid_clear_push(player);
				}
			}
				
			// HORIZONTAL COLLISION
				
			else
			{
				// If failed collide horizontally, clear push flag
				if (!(SharedData.player_physics >= PHYSICS_S3 || Mathf.Abs(yClip) > 4))
				{
					obj_act_solid_clear_push(player);
					return;
				}
				
				// If player is grounded, set their push flag (facing check isn't in the original engine)
				if (player.IsGrounded && Mathf.Sign(player.facing) == Mathf.Sign(objXf - pxf))
				{
					player.is_pushing = id;
				}
				else
				{
					player.is_pushing = false;
				}
				
				// Set collision flag
				if (pxf < objXf)
				{
					SolidData.touch_flags[pid] = 3;
				}
				else
				{
					SolidData.touch_flags[pid] = 4;
				}
				
				// Clip player out and reset their speeds
				if (xClip != 0 && Mathf.Sign(xClip) == Mathf.Sign(player.xsp))
				{
					player.gsp = 0;
					player.xsp = 0;
				}
						
				player.x -= xClip;
			}
		}
		
		// Is player trying to collide with platform object while moving down?
		else if (player.ysp >= 0)
		{
			// If player isn't within the object, return
			float relX = Mathf.Floor(px - objX) + objW;
			if (relX < 0 - extX || relX > objW * 2 + extX)
			{
				return;
			}
			
			// If player is above the object's top, return
			float objTop = Mathf.Floor(objY - objH);
			float playerBottom = Mathf.Floor(py + ph) + gripY;
			
			if (objTop > playerBottom)
			{
				return;
			}
			
			// If player isn't clipping into the object way too much, let them land
			float yClip = objTop - playerBottom;
			if (yClip >= -16 && yClip < 0)
			{
				obj_act_solid_land(player, id, _type, -yClip - gripY);
				
				// Set collision flag
				SolidData.touch_flags[pid] = 1;
			}
		}
	}
    
	private void LandOnSolid(OrbinautFramework3.Objects.Player.Player player, CommonObject targetObject, Constants.SolidType type, int distance)
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
				
		if (!player.IsGrounded)
		{
			player.IsGrounded = true;

			player.Land();
		}
	}
    
	private void ClearPush(OrbinautFramework3.Objects.Player.Player player)
	{
		if (player.IsPushing != id) return;
		if (player.Animation != PlayerConstants.Animation.Spin)
		{
			player.Animation = PlayerConstants.Animation.Move;
		}
				
		player.IsPushing = false;
	}
}