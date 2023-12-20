using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileSearcher(bool isVertical, Vector2I position, CollisionTileMap tileMap, 
    ushort tileLayerId, DirectionSign directionSign, GroundMode groundMode)
{
    public FoundTileData Search(int shift)
    {
        Vector2I shiftedPosition = position;
        if (isVertical)
        {
            shiftedPosition.Y += shift * (sbyte)directionSign;	
        }
        else
        {
            shiftedPosition.X += shift * (sbyte)directionSign;	
        }
        
        Vector2I mapPosition = tileMap.LocalToMap(shiftedPosition);
        int index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
        var transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
        byte size = GetTileCollision(isVertical, shiftedPosition, index, transforms);
        bool isValid = GetTileValidity(index, isVertical, directionSign, groundMode);

        return new FoundTileData(index, transforms, isValid, size);
    }
    
    private static byte GetTileCollision(bool isHeight, Vector2I position, int index, TileTransforms tileTransforms)
    {
        index %= TileLimit;
        if (index <= 0) return 0;
        var collisionIndex = (byte)((isHeight ? position.X : position.Y) % TileSize);
	
        if (tileTransforms.IsMirrored)
        {
            collisionIndex = (byte)(TileSize - 1 - collisionIndex);
        }
		
        return tileTransforms.IsRotated ^ isHeight ?
            FrameworkData.TileData.Heights[index][collisionIndex] : 
            FrameworkData.TileData.Widths[index][collisionIndex];
    }
    
    private static bool GetTileValidity(int index, bool isVertical, DirectionSign directionSign, GroundMode groundMode)
    {
        return (index / TileLimit) switch
        {
            2 when isVertical => groundMode switch
            {
                GroundMode.Floor => directionSign == DirectionSign.Negative,
                GroundMode.Ceiling => directionSign == DirectionSign.Positive,
                _ => true
            },
            1 when isVertical => groundMode switch
            {
                GroundMode.Floor => directionSign == DirectionSign.Positive,
                GroundMode.Ceiling => directionSign == DirectionSign.Negative,
                _ => false
            },
            2 => groundMode switch
            {
                GroundMode.RightWall => directionSign == DirectionSign.Negative,
                GroundMode.LeftWall => directionSign == DirectionSign.Positive,
                _ => true
            },
            1 => groundMode switch
            {
                GroundMode.RightWall => directionSign == DirectionSign.Positive,
                GroundMode.LeftWall => directionSign == DirectionSign.Negative,
                _ => false
            },
            _ => true
        };
    }
}
