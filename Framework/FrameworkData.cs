using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player;
using TileData = OrbinautFramework3.Framework.Tiles.TileData;

namespace OrbinautFramework3.Framework;

public static class FrameworkData
{
    public static double ProcessSpeed { get; set; }
    
    public static List<KeyboardControl> KeyboardControl { get; set; }
    public static bool GamepadVibration { get; set; }
    public static TileData TileData { get; }
    public static bool CDTileFixes { get; set; }
    public static bool CDCamera { get; set; }
    public static bool UpdateGraphics { get; set; }
    public static bool UpdateObjects { get; set; }
    public static bool UpdateTimer { get; set; }
    public static bool AllowPause { get; set; }
    public static CheckpointData CheckpointData { get; set; }
    public static Vector2I? GiantRingData { get; set; }
    public static PlayerBackupData PlayerBackupData { get; set; }
    public static CommonScene CurrentScene { get; set; }
    public static Vector2I ViewSize { get; set; }
    public static PlayerConstants.Type PlayerType { get; set; }
    public static PlayerConstants.Type PlayerAIType { get; set; }
    public static int RotationMode { get; set; }
    public static uint SavedScore { get; set; }
    public static uint SavedRings { get; set; }
    public static uint SavedLives { get; set; }
    public static Constants.Barrier SavedBarrier { get; set; }
    public static bool PlayerEditMode { get; set; }
    public static bool DeveloperMode { get; set; }
    public static bool IsPaused { get; set; }
    public static PlayerConstants.PhysicsType PlayerPhysics { get; set; }

    static FrameworkData()
    {
        GD.Randomize();

        KeyboardControl = new List<KeyboardControl>
        {
            new(Key.Up, Key.Down, Key.Left, Key.Right, 
                Key.A, Key.S, Key.D, Key.Enter, Key.Space),
            new(Key.None, Key.None, Key.None, Key.None, 
                Key.Z, Key.X, Key.C, Key.None, Key.None)
        };
        GamepadVibration = true;
        UpdateGraphics = true;
        UpdateObjects = true;
        UpdateTimer = true;
        AllowPause = true;
        CDTileFixes = true;
        CDCamera = true;
        TileData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        ViewSize = new Vector2I(400, 224);

        PlayerType = PlayerConstants.Type.Sonic;
        PlayerAIType = PlayerConstants.Type.Tails;
        
        RotationMode = 1;

        DeveloperMode = true;
        IsPaused = true;
        PlayerPhysics = PlayerConstants.PhysicsType.S2;
    }
}