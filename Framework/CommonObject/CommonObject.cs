using System;
using System.Collections.Generic;
using Godot;

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
    public AnimatedSprite Sprite { get; set; }
 
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

    public void ActSolid(Player player, Constants.SolidType type)
    {
	    // The following is long and replicates the method of colliding
		// with an object from the original games

		// Get player ID
		var _pid = player.player_id;
		
		// Clear collision flag
		data_solid.touch_flags[_pid] = 0;
		
		// Exit if can't collide
		if player.data_solid.radius_x <= 0 or player.data_solid.radius_y <= 0 or !player.object_interaction 
		{
			exit;
		}
		
		if data_solid.radius_x <= 0 or data_solid.radius_y <= 0
		{
			exit;
		}
		
		// Get player data
		var _px = player.x;
		var _py = player.y;
		var _pw = player.data_solid.radius_x;
		var _ph = player.data_solid.radius_y;
		
		var _pxf = floor(_px);
		var _pyf = floor(_py);
		
		// Get object data
		var _obj_x_prev = data_solid.offset_x + xprevious;
		var _obj_x = data_solid.offset_x + x;
		var _obj_y = data_solid.offset_y + y;
		var _obj_w = data_solid.radius_x;
		var _obj_h = data_solid.radius_y;
		var _obj_hmap = data_solid.height_map;
		
		var _obj_xf = floor(_obj_x);
		var _obj_yf = floor(_obj_y);
		
		var _combined_width  = _obj_w + player.data_solid.radius_x + 1;
		var _combined_height = _obj_h + player.data_solid.radius_y;
		
		var _slope_offset = 0;
		var _grip_y = 4;
		var _ext_x = 0;
		var _ext_y = 0;
		
		// Calculate offset for a sloped object
		if array_length(_obj_hmap) > 0
		{
			var _index;
			
			if image_xscale >= 0
			{
				_index = floor(_px - _obj_x) + _obj_w;
			}
			else
			{
				_index = floor(_obj_x - _px) + _obj_w;
			}	
			
			_index = clamp(_index, 0, array_length(_obj_hmap) - 1);
			
			_slope_offset = (_obj_h - _obj_hmap[_index]) * image_yscale;	
		}
		else
		{
			_slope_offset = 0;
		}
		
		// Extend collision box
		if global.better_solid_collision
		{
			_ext_x = _pw;
			_ext_y = _grip_y;
		}
		
		// Add collision check to the debug list
		if global.debug_collision == 3
		{
			var _ds_list = c_engine.collision.ds_solids;
			
			if ds_list_find_index(_ds_list, id) == -1
			{
				ds_list_add(_ds_list, _obj_x - _obj_w, _obj_y - _obj_h + _slope_offset, _obj_x + _obj_w, _obj_y + _obj_h + _slope_offset, id);
			}
			
			if ds_list_find_index(_ds_list, player) == -1
			{
				ds_list_add(_ds_list, _px - _pw, _py - _ph, _px + _pw, _py + _ph, player);
			}
		}
		
		// Is player standing on this object?
		if player.is_on_object == id
		{	
			// Set collision flag
			data_solid.touch_flags[_pid] = 1;
			
			// Move player with the object
			player.x += _obj_x - _obj_x_prev;
			player.y = _obj_y - _obj_h + _slope_offset - _ph - 1;
			
			_px = player.x;
			
			// Is player still within the object?
			if _type != SOBJ_TOP
			{
				var _rel_x = floor(_px - _obj_x) + _combined_width;
				if _rel_x > 0 and _rel_x < _combined_width * 2
				{
					exit;
				}
			}
			else
			{
				var _rel_x = floor(_px - _obj_x) + _obj_w;
				if _rel_x >= 0 - _ext_x and _rel_x <= _obj_w * 2 + _ext_x
				{
					exit;
				}
			}
			
			// If not, clear collision flag
			data_solid.touch_flags[_pid] = 0;
			
			// Clear player's flag
			player.is_on_object = false;
		}
		
		// Is player trying to collide with non-platform object?
		else if _type != SOBJ_TOP
		{
			// Is player within the object area?
			var _x_dist = floor(_px - _obj_x) + _combined_width;
			var _y_dist = floor(_py - _obj_y) + _combined_height - _slope_offset + _grip_y;
			
			// If not, clear push flag and exit
			if _x_dist < 0 or _x_dist > _combined_width  * 2 
			or _y_dist < 0 or _y_dist >= _combined_height * 2 + _ext_y
			{
				obj_act_solid_clear_push(player);
				exit;
			}
			
			// Calculate clip distance
			var _x_clip = _pxf < _obj_xf ? _x_dist : _x_dist - _combined_width  * 2;
			var _y_clip = _pyf < _obj_yf ? _y_dist : _y_dist - _combined_height * 2 - _grip_y;
			
			// Define if player should collide vertically
			var _v_collision = false;
			
			if _type != SOBJ_SIDES
			{
				if abs(_x_clip) >= abs(_y_clip)
				{
					_v_collision = true;
				}
				
				if global.player_physics >= PHYSICS_S3 and _y_clip <= 4
				{
					_v_collision = true;
				}
			}
				
			// Try to perform vertical collision
			if _v_collision
			{	
				// Try to collide from below
				if _y_clip < 0 and _type != SOBJ_ITEMBOX
				{
					// If player is standing on the ground, kill them
					if player.ysp == 0 and player.IsGrounded
					{
						if abs(_x_clip) >= 16
						{
							player_kill(player);
						}
					}
					
					// Else just clip player out
					else if player.ysp < 0
					{
						if global.player_physics >= PHYSICS_S3 and !player.IsGrounded
						{
							player.gsp = 0;
						}
								
						player.y -= _y_clip;
						player.ysp = 0;
						
						// Set collision flag
						data_solid.touch_flags[_pid] = 2;
					}
				}
				
				// Try to collide from above
				else if _y_clip >= 0 and _y_clip < 16
				{
					if player.ysp < 0
					{
						exit;
					}
					
					// If player is within the object and moving down, let them land
					var _rel_x = floor(_px - _obj_x) + _obj_w;
					if _rel_x >= 0 - _ext_x and _rel_x <= _obj_w * 2 + _ext_x
					{
						obj_act_solid_land(player, id, _type, _y_clip - _grip_y);
						
						// Set collision flag
						data_solid.touch_flags[_pid] = 1;
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
				if !(global.player_physics >= PHYSICS_S3 or abs(_y_clip) > 4)
				{
					obj_act_solid_clear_push(player);
					exit;
				}
				
				// If player is grounded, set their push flag (facing check isn't in the original engine)
				if player.IsGrounded and sign(player.facing) == sign(_obj_xf - _pxf)
				{
					player.is_pushing = id;
				}
				else
				{
					player.is_pushing = false;
				}
				
				// Set collision flag
				if _pxf < _obj_xf
				{
					data_solid.touch_flags[_pid] = 3;
				}
				else
				{
					data_solid.touch_flags[_pid] = 4;
				}
				
				// Clip player out and reset their speeds
				if _x_clip != 0 and sign(_x_clip) == sign(player.xsp)
				{
					player.gsp = 0;
					player.xsp = 0;
				}
						
				player.x -= _x_clip;
			}
		}
		
		// Is player trying to collide with platform object while moving down?
		else if player.ysp >= 0
		{
			// If player isn't within the object, exit
			var _rel_x = floor(_px - _obj_x) + _obj_w;
			if _rel_x < 0 - _ext_x or _rel_x > _obj_w * 2 + _ext_x
			{
				exit;
			}
			
			// If player is above the object's top, exit
			var _obj_top = floor(_obj_y - _obj_h);
			var _player_bottom = floor(_py + _ph) + _grip_y;
			
			if _obj_top > _player_bottom
			{
				exit;
			}
			
			// If player isn't clipping into the object way too much, let them land
			var _y_clip = _obj_top - _player_bottom;
			if _y_clip >= -16 and _y_clip < 0
			{
				obj_act_solid_land(player, id, _type, -_y_clip - _grip_y);
				
				// Set collision flag
				data_solid.touch_flags[_pid] = 1;
			}
		}
    }
    
    private void LandOnSolid(Player player, CommonObject targetObject, Constants.SolidType type, int distance)
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
    
    private void ClearPush(Player player)
    {
	    if (player.IsPushing != id) return;
	    if (player.Animation != PlayerConstants.Animation.Spin)
	    {
		    player.Animation = PlayerConstants.Animation.Move;
	    }
				
	    player.IsPushing = false;
    }
}
