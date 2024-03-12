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

    public override void _Process(double delta)
    {
        GD.Print(GetCellAlternativeTile(_layerId, (Vector2I)GetGlobalMousePosition() / 16));
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
            atlasCoords = atlasCoords == Vector2I.Zero ? -Vector2I.One : atlasCoords;
            index = int.Parse(flipLine) * 8192 + int.Parse(mirrorLine) * 4096;
            SetCell(_layerId, position, _sourceId, atlasCoords, index);
            
            position.X++;
            position.Y += position.X / width;
            position.X %= width;
        }
    
        reader.Close();
    }
}
