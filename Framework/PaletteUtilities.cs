using System.Collections.Generic;
using System.Linq;

namespace OrbinautFramework3.Framework;

public static class PaletteUtilities
{
    public const ushort PaletteLimit = 256;
    
    public static List<int> Colors { get; }
    public static int[] Duration { get; }
    public static int[] Timer { get; }
    public static int[] Last { get; }
    public static int[] Loop { get; }
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
        Last = new int[PaletteLimit];
        Loop = new int[PaletteLimit];
        Index = Enumerable.Repeat(1, PaletteLimit).ToArray();
        ColourId =
        [
            Enumerable.Repeat(1, PaletteLimit).ToArray(), 
            Enumerable.Repeat(1, PaletteLimit).ToArray()
        ];
        SplitBound = FrameworkData.ViewSize.Y;
        UpdateFlag = true;
    }

    public static void SetRotation(IEnumerable<int> colorsId, int loopIndex, int endIndex, int duration)
    {
        foreach (int colorId in colorsId)
        {
            Colors.Add(colorId);

            Loop[colorId] = loopIndex;
            Last[colorId] = endIndex;
            Duration[colorId] = duration;
        }
    }
}