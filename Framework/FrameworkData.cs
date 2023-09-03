using Godot;
using System;
using System.Collections.Generic;

public static class FrameworkData
{
    public static List<KeyboardControl> KeyboardControl { get; set; }
    public static bool GamepadVibration { get; set; }
    public static TileData TileData { get; }
    public static bool CDTileFixes { get; set; }
    public static bool CDCamera { get; set; }
    public static bool UpdateGraphics { get; set; }
    public static bool UpdateObjects { get; set; }
    public static CheckpointData CheckpointData { get; set; }
    public static CommonScene CurrentScene { get; set; }
    public static Vector2I ViewSize { get; set; }
    public static PlayerConstants.Type PlayerType { get; set; }
    public static PlayerConstants.Type PlayerAIType { get; set; }
    public static int RotationMode { get; set; }

    static FrameworkData()
    {
        KeyboardControl = new List<KeyboardControl>
        {
            new(Key.Up, Key.Down, Key.Left, Key.Right, Key.A, Key.S, Key.D, Key.Enter),
            new(Key.None, Key.None, Key.None, Key.None, Key.Z, Key.X, Key.C, Key.Space)
        };
        GamepadVibration = true;
        UpdateGraphics = true;
        UpdateObjects = true;
        CDTileFixes = true;
        CDCamera = true;
        TileData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        ViewSize = new Vector2I(400, 224);

        PlayerType = PlayerConstants.Type.Sonic;
        PlayerAIType = PlayerConstants.Type.Tails;
        
        //TODO: Ask Triangly
        RotationMode = 1;
    }
}
