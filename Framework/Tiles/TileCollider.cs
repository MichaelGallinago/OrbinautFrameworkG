using Godot;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Framework.Tiles;

public class TileCollider
{
    private Vector2I _position;
    private TileLayers _type;
    private CollisionTileMap _tileMap;
    private GroundMode _groundMode;
    
    public void SetData(Vector2I position, TileLayers type, 
        CollisionTileMap tileMap, GroundMode groundMode = GroundMode.Floor)
    {
        _position = position;
        _type = type;
        _tileMap = tileMap;
        _groundMode = groundMode;
    }

    public (int, float?) FindTile(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindTile(isVertical, 
            _position + offset, direction, _type, _tileMap, _groundMode);
    }
    
    public int FindDistance(Vector2I offset, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindDistance(isVertical, 
            _position + offset, direction, _type, _tileMap, _groundMode);
    }

    public (int, float?) FindClosestTile(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestTile(isVertical, 
            _position + offset1, _position + offset2, direction, _type, _tileMap, _groundMode);
    }
    
    public int FindClosestDistance(Vector2I offset1, Vector2I offset2, bool isVertical, Direction direction)
    {
        return CollisionUtilities.FindClosestDistance(isVertical, 
            _position + offset1, _position + offset2, direction, _type, _tileMap, _groundMode);
    }
}
