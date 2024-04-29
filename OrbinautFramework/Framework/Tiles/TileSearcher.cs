using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileSearcher(bool isVertical, Vector2I position, CollisionTileMap tileMap, 
    ushort tileLayerId, Direction direction, TileBehaviours tileBehaviours)
{
    public FoundTileData Search(int shift)
    {
        Vector2I shiftedPosition = position;
        if (isVertical)
        {
            shiftedPosition.Y += shift * (int)direction;
        }
        else
        {
            shiftedPosition.X += shift * (int)direction;
        }
        
        Vector2I mapPosition = tileMap.LocalToMap(shiftedPosition);
        int index = tileMap.GetTileIndex(tileMap.GetCellAtlasCoords(tileLayerId, mapPosition));
        var transforms = new TileTransforms(tileMap.GetCellAlternativeTile(tileLayerId, mapPosition));
        byte size = GetTileCollision(isVertical, shiftedPosition, index, transforms);
        bool isValid = GetTileValidity(index, isVertical, direction, tileBehaviours);

        return new FoundTileData(index, transforms, isValid, size);
    }
    
    private static byte GetTileCollision(bool isHeight, Vector2I position, int index, TileTransforms tileTransforms)
    {
        index %= TileLimit;
        if (index <= 0) return 0;
        var collisionIndex = (byte)((isHeight ? position.X : position.Y) % TileSize);
	
        if (isHeight && tileTransforms.IsMirrored || !isHeight && tileTransforms.IsFlipped)
        {
            collisionIndex = (byte)(TileSize - 1 - collisionIndex);
        }
		
        return tileTransforms.IsRotated ^ isHeight ? 
            CollisionUtilities.TilesData.Heights[index][collisionIndex] : 
            CollisionUtilities.TilesData.Widths[index][collisionIndex];
    }
    
    private static bool GetTileValidity(int index, bool isVertical, Direction direction, TileBehaviours tileBehaviours)
    {
        return (index / TileLimit) switch
        {
            2 => !CheckTileValidity(isVertical, direction, tileBehaviours),
            1 => CheckTileValidity(isVertical, direction, tileBehaviours),
            _ => true
        };
    }

    private static bool CheckTileValidity(bool isVertical, Direction direction, TileBehaviours tileBehaviours)
    {
        return tileBehaviours switch
        {
            TileBehaviours.Floor => isVertical && direction == Direction.Positive,
            TileBehaviours.Ceiling => isVertical && direction == Direction.Negative,
            TileBehaviours.RightWall => !isVertical && direction == Direction.Positive,
            TileBehaviours.LeftWall => !isVertical && direction == Direction.Negative,
            _ => false
        };
    }
}
