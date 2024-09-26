using Godot;

namespace OrbinautFrameworkG.Framework.Tiles;

public partial class CollisionTileMap : TileMapLayer
{
    private int _columnCount;

    public CollisionTileMap()
    {
        _columnCount = GetTileSetColumnCount(0);
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
