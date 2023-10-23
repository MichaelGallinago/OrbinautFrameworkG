namespace OrbinautFramework3.Framework.Tiles;

public class TileTransforms
{
    public readonly bool IsRotated;
    public readonly bool IsFlipped;
    public readonly bool IsMirrored;

    public TileTransforms(int alternativeId)
    {
        IsRotated = alternativeId >> 14 > 0;
        IsFlipped = (alternativeId & 16383) >> 13 > 0;
        IsMirrored = (alternativeId & 8191) >> 12 > 0;
    }
}