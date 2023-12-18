namespace OrbinautFramework3.Framework.Tiles;

public struct FoundTileData(int index, TileTransforms transforms, bool isValid, byte size)
{
    public readonly int Index = index;
    public readonly TileTransforms Transforms = transforms;
    public readonly bool IsValid = isValid;
    public byte Size = size;
}