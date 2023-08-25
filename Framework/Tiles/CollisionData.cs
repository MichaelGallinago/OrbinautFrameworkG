using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public struct TileData
{
    public readonly byte[][] Heights;
    public readonly byte[][] Widths;
    public readonly float[] Angles;

    public TileData(byte[][] heights, byte[][] widths, float[] angles)
    {
        Heights = heights;
        Widths = widths;
        Angles = angles;
    }
}
