namespace OrbinautFramework3.Framework.Tiles;

public struct TilesData(byte[][] heights, byte[][] widths, float[] angles)
{
    public readonly byte[][] Heights = heights;
    public readonly byte[][] Widths = widths;
    public readonly float[] Angles = angles;
}