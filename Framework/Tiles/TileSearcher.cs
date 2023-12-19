using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileSearcher(bool isVertical, Vector2I position, CollisionTileMap tileMap, 
    ushort tileLayerId, Direction direction, GroundMode groundMode)
{
    public FoundTileData Search(int shift)
    {
        Vector2I shiftedPosition = position;
        if (isVertical)
        {
            shiftedPosition.Y += shift * (sbyte)direction;	
        }
        else
        {
            shiftedPosition.X += shift * (sbyte)direction;	
        }
        
        Vector2I mapPosition = tileMap.LocalToMap(shiftedPosition);
        int index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
        var transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
        byte size = GetTileCollision(isVertical, shiftedPosition, index, transforms);
        bool isValid = GetTileValidity(index, isVertical, direction, groundMode);

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
    
    private static bool GetTileValidity(int index, bool isVertical, 
        Direction direction, GroundMode groundMode)
    {
        return (index / TileLimit) switch
        {
            2 when isVertical => groundMode switch
            {
                GroundMode.Floor => direction == Direction.Negative,
                GroundMode.Ceiling => direction == Direction.Positive,
                _ => true
            },
            1 when isVertical => groundMode switch
            {
                GroundMode.Floor => direction == Direction.Positive,
                GroundMode.Ceiling => direction == Direction.Negative,
                _ => false
            },
            2 => groundMode switch
            {
                GroundMode.RightWall => direction == Direction.Negative,
                GroundMode.LeftWall => direction == Direction.Positive,
                _ => true
            },
            1 => groundMode switch
            {
                GroundMode.RightWall => direction == Direction.Positive,
                GroundMode.LeftWall => direction == Direction.Negative,
                _ => false
            },
            _ => true
        };
    }
}
