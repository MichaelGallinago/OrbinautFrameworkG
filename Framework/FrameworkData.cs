using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Barrier;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public static class FrameworkData
{
    public static float ProcessSpeed { get; set; }
    public static List<KeyboardControl> KeyboardControl { get; set; }
    public static bool GamepadVibration { get; set; }
    public static TilesData TilesData { get; }
    public static bool UpdateAnimations { get; set; }
    public static bool UpdateEffects { get; set; }
    public static bool UpdateObjects { get; set; }
    public static bool LastUpdateObjects { get; set; }
    public static bool UpdateTimer { get; set; }
    public static bool AllowPause { get; set; }
    public static bool DropDash { get; set; }
    public static PlayerBackupData PlayerBackupData { get; set; }
    public static CommonScene CurrentScene { get; set; }
    public static int RotationMode { get; set; }
    public static uint SavedScore { get; set; }
    public static uint SavedRings { get; set; }
    public static uint SavedLives { get; set; }
    public static Barrier.Types SavedBarrier { get; set; }
    public static bool PlayerEditMode { get; set; }
    public static bool DeveloperMode { get; set; }
    public static bool IsPaused { get; set; }
    public static PhysicsTypes PlayerPhysics { get; set; }
    public static double Time { get; set; }

    static FrameworkData()
    {
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
        DropDash = true;
        TilesData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        
        RotationMode = 1;

        DeveloperMode = true;
        IsPaused = false;
        PlayerPhysics = PhysicsTypes.S2;
    }
    
    public static bool IsTimePeriodLooped(float period) => Time % period - ProcessSpeed < 0f;
    
    public static void UpdateEarly(float processSpeed)
    {
        if (AllowPause && InputUtilities.Press[0].Start)
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
		}
		
		if (UpdateTimer && !IsPaused)
		{
			Time += processSpeed;
		}
		
		if (LastUpdateObjects != UpdateObjects)
		{
			// Whenever update_objects is set from false to true, activate ALL objects
			// (needed to make BEHAVE_NOBOUNDS objects work correctly)
			if (UpdateObjects)
			{
				foreach (BaseObject commonObject in BaseObject.Objects)
				{
					commonObject.SetActivity(true);
				}
			}
		
			LastUpdateObjects = UpdateObjects;
		}
	
		if (!UpdateObjects || IsPaused)
		{
			// Deactivate objects
			foreach (BaseObject commonObject in BaseObject.Objects)
			{
				if (commonObject.Behaviour == BaseObject.BehaviourType.Unique) return;
				commonObject.SetActivity(false);
			}

			return;
		}
		
		// Deactivate or reset objects outside the new active area
		Vector2I activeArea = Camera.Main.GetActiveArea();
		int limitBottom = Camera.Main.LimitBottom;
		
		foreach (BaseObject commonObject in BaseObject.Objects)
		{
			DeactivateObjectsByBehaviour(commonObject, limitBottom, ref activeArea);
			
			// Activate objects within the new active area and reset interaction flag for all active objects
			if (commonObject.Position.X < activeArea.X  || commonObject.Position.Y < 0f || 
			    commonObject.Position.X >= activeArea.Y || commonObject.Position.Y >= limitBottom) continue;
			commonObject.SetActivity(true);
			commonObject.InteractData.IsInteract = true;
		}
    }
    
    private static void DeactivateObjectsByBehaviour(BaseObject commonObject, int limitBottom, ref Vector2I activeArea)
    {		
	    switch (commonObject.Behaviour)
	    {
		    case BaseObject.BehaviourType.NoBounds:
		    case BaseObject.BehaviourType.Unique:
			    break;
					
		    case BaseObject.BehaviourType.Delete:
			    Vector2 position = commonObject.Position;
			    if (position.X < activeArea.X || position.X > activeArea.Y || 
			        position.Y < 0 || position.Y > limitBottom)
			    {
				    commonObject.QueueFree();
			    }
			    break;
					
		    case BaseObject.BehaviourType.Reset:
			    if (commonObject.Position.X >= activeArea.X && commonObject.Position.X <= activeArea.Y) break;
			    
			    float resetX = commonObject.RespawnData.Position.X;
			    if (resetX >= activeArea.X && resetX <= activeArea.Y)
			    {
				    commonObject.Position = new Vector2(sbyte.MinValue, sbyte.MinValue);
				    commonObject.Hide();
						
				    break;
			    }
			    
			    commonObject.Reset();
			    commonObject.SetActivity(false);
			    break;
				
		    default: 
			    if (commonObject.Position.X >= activeArea.X && commonObject.Position.X <= activeArea.Y) break;
					
			    float respawnX = commonObject.RespawnData.Position.X;
			    if (respawnX >= activeArea.X && respawnX <= activeArea.Y) break;
			    commonObject.SetActivity(false);
			    break;
	    }
    }
}
