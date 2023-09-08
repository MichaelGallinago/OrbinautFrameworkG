using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class PaletteUtilities
{
    public const ushort PaletteLimit = 256;
    
    public static List<Color> Colors = new();
    public static int[] Duration { get; }
    public static int[] Timer { get; }
    public static int[] Last { get; }
    public static int[] Loop { get; }
    public static int[] Index { get; }
    public static int[][] ColourId { get; }
    public static int[][] ColourSet { get; }
    public static int SplitBound { get; }
    public static bool UpdateFlag { get; }

    static PaletteUtilities()
    {
        Duration = new int[PaletteLimit];
        Timer = new int[PaletteLimit];
        Last = new int[PaletteLimit];
        Loop = new int[PaletteLimit];
        Index = Enumerable.Repeat(1, PaletteLimit).ToArray();
        ColourId = new[] 
        {
            Enumerable.Repeat(1, PaletteLimit).ToArray(), 
            Enumerable.Repeat(1, PaletteLimit).ToArray() 
        };
        SplitBound = FrameworkData.ViewSize.Y;
        UpdateFlag = true;
    }
}
