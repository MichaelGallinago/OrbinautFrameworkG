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
            
            if (indexLine == null || flipLine == null || mirrorLine == null) break;

            int index = int.Parse(indexLine);
            var atlasCoords = new Vector2I(index % columnCount, index / columnCount);
            index += int.Parse(flipLine) * 16384 + int.Parse(mirrorLine) * 8192;
            SetCell(_layerId, position, 0, atlasCoords, index);
            position.X = (position.X + 1) % width;
        }
        
        reader.Close();
    }
}
