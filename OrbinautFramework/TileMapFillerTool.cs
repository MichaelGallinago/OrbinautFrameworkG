using System.IO;
using Godot;

namespace OrbinautFramework3;

[Tool]
public partial class TileMapFillerTool : TileMap
{
    [Export] private byte _layerId;
    [Export] private string _filePath;
    
    [Export] private bool Fill
    {
        get => _fill;
        set
        {
            _fill = false;
            FillTileMap();
        }
    }

    private bool _fill;

    public void FillTileMap()
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
        string index, flip, mirror;
        while (true)
        {
            index = reader.ReadLine();
            flip = reader.ReadLine();
            mirror = reader.ReadLine();
            
            if (index == null || flip == null || mirror == null) break;
            
            SetCell(_layerId, position, int.Parse(index));
            position.X = (position.X + 1) % width;
        }
        
        reader.Close();
    }
}