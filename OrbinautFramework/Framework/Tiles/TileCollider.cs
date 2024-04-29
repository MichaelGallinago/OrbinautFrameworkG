using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileCollider
{
    public Vector2I Position;
    public TileBehaviours TileBehaviours;
    
    private TileLayers _type;
    private CollisionTileMap _tileMap;
    
    public void SetData(Vector2I position, TileLayers type, 
        CollisionTileMap tileMap, TileBehaviours tileBehaviours = TileBehaviours.Floor)
    {
        Position = position;
        TileBehaviours = tileBehaviours;
        
        _type = type;
        _tileMap = tileMap;
    }

    public (int, float) FindTile(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindTile(isVertical, 
            Position + offset, direction, _type, _tileMap, TileBehaviours);
    }
    
    public int FindDistance(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindDistance(isVertical, 
            Position + offset, direction, _type, _tileMap, TileBehaviours);
    }

    public (int, float) FindClosestTile(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestTile(isVertical, 
            Position + offset1, Position + offset2, direction, _type, _tileMap, TileBehaviours);
    }
    
    public int FindClosestDistance(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestDistance(isVertical, 
            Position + offset1, Position + offset2, direction, _type, _tileMap, TileBehaviours);
    }
}
