using Godot;
using System;
using static Constants;

public partial class CollisionTileMap : TileMap
{
    private int _columnCount;
    
    public override void _Ready()
    {
        _columnCount = GetTileSetColumnCount(0);
    }
    
    public override void _Process(double delta)
    {
        Vector2I position = LocalToMap(GetLocalMousePosition());
        
        Vector2I atlasCoords = GetCellAtlasCoords(0, position);
        int tileId = GetTileIndex(atlasCoords);

        CollisionUtilities.FindTile(true, position, Direction.Positive, TileLayer.Main, this);
        var tileTransforms = new TileTransforms(GetCellAlternativeTile(0, position));
        float rawAngle = CollisionUtilities.GetRawTileAngle(tileId);
        float angle = Angles.TransformTileAngle(rawAngle, tileTransforms);
        
        GD.Print($"{tileId} Angle:{angle} RawAngle:{rawAngle}");
    }

    public int GetTileIndex(Vector2I atlasCoords)
    {
        return (atlasCoords.X + atlasCoords.Y * _columnCount) % TileLimit;
    }

    private int GetTileSetColumnCount(int sourceId)
    {
        TileSetSource source = TileSet.GetSource(sourceId);
        var atlasCoords = new Vector2I();
        while (source.HasTile(atlasCoords))
        {
            atlasCoords.X++;
        }

        return atlasCoords.X;
    }
}
