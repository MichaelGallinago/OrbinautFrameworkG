using Godot;
using System;
using System.Collections.Generic;

public static class FrameworkData
{
    public static List<KeyboardControl> KeyboardControl { get; set; }
    public static bool GamepadVibration { get; set; }
    public static TileData TileData { get; }
    public static bool CDTileFixes { get; set; }

    static FrameworkData()
    {
        KeyboardControl = new List<KeyboardControl>
        {
            new(Key.Up, Key.Down, Key.Left, Key.Right, Key.A, Key.S, Key.D, Key.Enter),
            new(Key.None, Key.None, Key.None, Key.None, Key.Z, Key.X, Key.C, Key.Space)
        };
        GamepadVibration = true;
        CDTileFixes = true;
        TileData = CollisionUtilities.LoadTileDataBinary(
            "angles_s1", "heights_s1", "widths_s1");
    }
}
