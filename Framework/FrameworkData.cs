using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using Player = OrbinautFramework3.Objects.Player.Player;
using TileData = OrbinautFramework3.Framework.Tiles.TileData;

namespace OrbinautFramework3.Framework;

public static class FrameworkData
{
    public static float ProcessSpeed { get; set; }
    
    public static List<KeyboardControl> KeyboardControl { get; set; }
    public static bool GamepadVibration { get; set; }
    public static TileData TileData { get; }
    public static bool CDTileFixes { get; set; }
    public static bool CDCamera { get; set; }
    public static bool UpdateAnimations { get; set; }
    public static bool UpdateEffects { get; set; }
    public static bool UpdateObjects { get; set; }
    public static bool UpdateTimer { get; set; }
    public static bool AllowPause { get; set; }
    public static bool DropDash { get; set; }
    public static CheckpointData CheckpointData { get; set; }
    public static Vector2I? GiantRingData { get; set; }
    public static PlayerBackupData PlayerBackupData { get; set; }
    public static CommonScene CurrentScene { get; set; }
    public static Vector2I ViewSize { get; set; }
    public static Player.Types PlayerType { get; set; }
    public static Player.Types PlayerAIType { get; set; }
    public static int RotationMode { get; set; }
    public static uint SavedScore { get; set; }
    public static uint SavedRings { get; set; }
    public static uint SavedLives { get; set; }
    public static Barrier.Types SavedBarrier { get; set; }
    public static bool PlayerEditMode { get; set; }
    public static bool DeveloperMode { get; set; }
    public static bool IsPaused { get; set; }
    public static Player.PhysicsTypes PlayerPhysics { get; set; }
    public static double Time { get; set; }

    static FrameworkData()
    {
        GD.Randomize();

        KeyboardControl =
        [
            new KeyboardControl(Key.Up, Key.Down, Key.Left, Key.Right, 
                Key.A, Key.S, Key.D, Key.Enter, Key.Space),

            new KeyboardControl(Key.None, Key.None, Key.None, Key.None,
                Key.Z, Key.X, Key.C, Key.None, Key.None)
        ];
        GamepadVibration = true;
        UpdateAnimations = true;
        UpdateEffects = true;
        UpdateObjects = true;
        UpdateTimer = true;
        AllowPause = true;
        CDTileFixes = true;
        CDCamera = true;
        DropDash = true;
        TileData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        ViewSize = new Vector2I(400, 224);

        PlayerType = Player.Types.Sonic;
        PlayerAIType = Player.Types.Tails;
        
        RotationMode = 1;

        DeveloperMode = true;
        IsPaused = false;
        PlayerPhysics = Player.PhysicsTypes.S2;
    }
    
    private static void UpdateEarly(float processSpeed)
    {
	    /*
        if (AllowPause && input.press[0].start)
		{
			IsPaused = !IsPaused;
			
			//TODO: audio
			/*
			if (FrameworkData.IsPaused)
			{	
				audio_pause_all();
			}
			else
			{
				audio_resume_all();
			}
			*/
	    /*
		}
		
		if (UpdateTimer && !IsPaused)
		{
			Time += processSpeed;
		}
		
		if (local_update_objects != UpdateObjects)
		{
			// Whenever update_objects is set from false to true, activate ALL objects (needed to make BEHAVE_NOBOUNDS objects work correctly)
			if (UpdateObjects)
			{
				instance_activate_all();
			}
		
			local_update_objects = UpdateObjects;
		}
	
		if (!UpdateObjects || IsPaused)
		{
			// Deactivate objects
			with c_object
			{
				if data_respawn.behaviour != BEHAVE_UNIQUE
				{
					instance_deactivate_object(id);
				}
			}
			
			exit;
		}
		
		// Deactivate or reset objects outside the new active area
		var _active_area = camera_get_active_area(camera.instance);
		with c_object
		{
			switch data_respawn.behaviour
			{
				case BEHAVE_NOBOUNDS:
				case BEHAVE_UNIQUE:
				
					continue;
					
				case BEHAVE_DELETE:
				
					if x < _active_area[0] || x > _active_area[1] || y < 0 || y > room_height
					{
						instance_destroy();
					}
					
				continue;
					
				case BEHAVE_RESET:
				
					if x >= _active_area[0] && x <= _active_area[1]
					{
						continue;
					}
			
					if data_respawn.start_x >= _active_area[0] && data_respawn.start_x <= _active_area[1]
					{
						x = -128;
						y = -128;
						visible = false;
						
						continue;
					}

					// Reset properties and re-initialise all variables
					x = data_respawn.start_x;
					y = data_respawn.start_y;
					image_xscale = data_respawn.scale_x;
					image_yscale = data_respawn.scale_y;
					image_index = data_respawn.img_index;
					sprite_index = data_respawn.spr_index;
					visible = data_respawn.is_visible;
					depth = data_respawn.priority;
						
					event_perform(ev_create, 0);
					
					instance_deactivate_object(id);
					
				continue;
				
				default: 
				
					if x >= _active_area[0] && x <= _active_area[1]
					{
						continue;
					}
					
					if data_respawn.start_x < _active_area[0] || data_respawn.start_x > _active_area[1]
					{
						instance_deactivate_object(id);
					}
					
				continue;
			}
		}
		
		// Activate objects within the new active area
		instance_activate_region(_active_area[0], 0, _active_area[1] - _active_area[0], room_height, true);
			
		// Reset interaction flag for all active objects
		with c_object
		{
			data_interact.interact = true;
		}
*/
    }
}