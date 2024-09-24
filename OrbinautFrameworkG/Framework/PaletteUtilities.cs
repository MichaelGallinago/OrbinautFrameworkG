using System;
using System.Collections.Generic;
using System.Linq;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework;

public static class PaletteUtilities
{
    public const ushort PaletteLimit = 256;
    
    public static List<int> Colors { get; }
    public static int[] Duration { get; }
    public static int[] Timer { get; }
    public static int[] EndIndex { get; }
    public static int[] LoopIndex { get; }
    public static int[] Index { get; }
    public static int[][] ColourId { get; }
    public static int[][] ColorSet { get; }
    public static int SplitBound { get; }
    public static bool UpdateFlag { get; }

    static PaletteUtilities()
    {
        Colors = [];
        Duration = new int[PaletteLimit];
        Timer = new int[PaletteLimit];
        EndIndex = new int[PaletteLimit];
        LoopIndex = new int[PaletteLimit];
        Index = Enumerable.Repeat(1, PaletteLimit).ToArray();
        ColourId =
        [
            Enumerable.Repeat(1, PaletteLimit).ToArray(), 
            Enumerable.Repeat(1, PaletteLimit).ToArray()
        ];
        SplitBound = Settings.ViewSize.Y;
        UpdateFlag = true;
    }

    public static void SetRotation(ReadOnlySpan<int> colorsId, int loopIndex, int endIndex, int duration)
    {
        foreach (int colorId in colorsId)
        {
            Colors.Add(colorId);

            LoopIndex[colorId] = loopIndex;
            EndIndex[colorId] = endIndex;
            Duration[colorId] = duration;
        }
    }
}