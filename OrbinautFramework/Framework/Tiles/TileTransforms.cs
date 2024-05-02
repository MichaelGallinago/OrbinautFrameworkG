namespace OrbinautFramework3.Framework.Tiles;

public readonly struct TileTransforms(int alternativeId)
{
    public readonly bool IsRotated = alternativeId >> 14 > 0;
    public readonly bool IsFlipped = (alternativeId & 16383) >> 13 > 0;
    public readonly bool IsMirrored = (alternativeId & 8191) >> 12 > 0;
}