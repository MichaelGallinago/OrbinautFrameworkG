using System.IO;
using Godot;

namespace OrbinautFramework3;

[Tool]
public partial class TileMapFillerTool : TileMap
{
    [Export] private byte _layerId;
    [Export] private byte _sourceId;
    [Export] private string _filePath;
    
    [Export] private bool Fill
    {
        get => false;
        set
        {
            if (value)
            {
                FillTileMap();
            }
        }
    }

    private void FillTileMap()
    {
        var reader = new StreamReader(_filePath);
    
        string size = reader.ReadLine();
    
        if (size == null)
        {
            reader.Close();
            return;
        }

        int width = int.Parse(size);
        Vector2I position = Vector2I.Zero;
        int columnCount = ((TileSetAtlasSource)TileSet.GetSource(_sourceId)).GetAtlasGridSize().X;
    
        while (true)
        {
            string indexLine = reader.ReadLine();
            string flipLine = reader.ReadLine();
            string mirrorLine = reader.ReadLine();
            string rotateLine = reader.ReadLine();
        
            if (indexLine == null || flipLine == null || mirrorLine == null || rotateLine == null) break;

            int index = int.Parse(indexLine);
            var atlasCoords = new Vector2I(index % columnCount, index / columnCount);
            atlasCoords = atlasCoords == Vector2I.Zero ? -Vector2I.One : atlasCoords;
            int rotate = int.Parse(rotateLine);
            int mirror = int.Parse(mirrorLine);
            int flip = int.Parse(flipLine);
            index = rotate == 0 ? flip * 8192 + mirror * 4096 : (1 - flip) * 4096 + mirror * 8192 + 16384;
            SetCell(_layerId, position, _sourceId, atlasCoords, index);
            
            position.X++;
            position.Y += position.X / width;
            position.X %= width;
        }
    
        reader.Close();
    }
}
