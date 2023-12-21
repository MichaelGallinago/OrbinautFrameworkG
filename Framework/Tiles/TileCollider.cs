using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileCollider
{
    public Vector2I Position;
    public TileLayers Type;
    public CollisionTileMap TileMap;
    public GroundMode GroundMode;
    
    public void SetData(Vector2I position, TileLayers type, 
        CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
    {
        Position = position;
        Type = type;
        TileMap = tileMap;
        GroundMode = groundMode;
    }

    public (int, float) FindTile(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindTile(isVertical, 
            Position + offset, direction, Type, TileMap, GroundMode);
    }
    
    public int FindDistance(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindDistance(isVertical, 
            Position + offset, direction, Type, TileMap, GroundMode);
    }

    public (int, float) FindClosestTile(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestTile(isVertical, 
            Position + offset1, Position + offset2, direction, Type, TileMap, GroundMode);
    }
    
    public int FindClosestDistance(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestDistance(isVertical, 
            Position + offset1, Position + offset2, direction, Type, TileMap, GroundMode);
    }
}
