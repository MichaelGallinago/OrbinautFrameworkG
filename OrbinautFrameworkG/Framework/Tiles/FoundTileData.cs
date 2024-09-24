namespace OrbinautFrameworkG.Framework.Tiles;

public struct FoundTileData(int index, sbyte shift, TileTransforms transforms, 
    bool isValid, byte size, bool isValidAngle = true)
{
    public readonly bool IsValidAngle = isValidAngle;
    public readonly int Index = index;
    public readonly TileTransforms Transforms = transforms;
    public readonly bool IsValid = isValid;
    public sbyte Shift = shift;
    public byte Size = size;
}
