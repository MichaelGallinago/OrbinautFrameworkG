using Godot;
using OrbinautFramework3.Framework;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3;

public partial class CollisionTileMap : TileMap
{
    private int _columnCount;
    
    public override void _Ready()
    {
        if (GetOwner<Node>() is CommonScene scene)
        {
            scene.CollisionTileMap = this;
        }
        
        _columnCount = GetTileSetColumnCount(0);
    }
    
    public override void _Process(double delta)
    {
        //var position = (Vector2I)GetLocalMousePosition();
        //(sbyte, float?) tile = CollisionUtilities.FindTile(
        //    true, position, Direction.Positive, TileLayer.Main, this);
        //GD.Print($"height:{tile.Item1} angle:{tile.Item2}");
        
        //var tileTransforms = new TileTransforms(GetCellAlternativeTile(0, position));
        //float rawAngle = CollisionUtilities.GetRawTileAngle(tileId);
        //float angle = Angles.TransformTileAngle(rawAngle, tileTransforms);
        //GD.Print($"{tileId} Angle:{angle} RawAngle:{rawAngle}");
    }

    public int GetTileIndex(Vector2I atlasCoords)
    {
        int index = atlasCoords.X + atlasCoords.Y * _columnCount;
        return index < 0 ? 0 : index;
    }

    private int GetTileSetColumnCount(int sourceId)
    {
        return ((TileSetAtlasSource)TileSet.GetSource(sourceId)).GetAtlasGridSize().X;
    }
}