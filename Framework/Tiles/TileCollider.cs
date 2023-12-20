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

    public (sbyte, float?) FindTile(Directions direction, Vector2I offset)
    {
        return CollisionUtilities.FindTile(
            CheckIsVertical(direction), _position + offset, 
            GetDirectionSign(direction), _type, _tileMap, _groundMode);
    }

    public (sbyte, float?) FindTileTwoPositions(
        Vector2I offset1, Vector2I offset2, Directions direction)
    {
        return CollisionUtilities.FindTileTwoPositions(
            CheckIsVertical(direction), _position + offset1, _position + offset2, 
            GetDirectionSign(direction), _type, _tileMap, _groundMode);
    }
    
    private static DirectionSign GetDirectionSign(Directions direction)
    {
        return direction <= Directions.Right ? DirectionSign.Positive : DirectionSign.Negative;
    }

    private static bool CheckIsVertical(Directions direction) => ((int)direction & 1) == 0;
}