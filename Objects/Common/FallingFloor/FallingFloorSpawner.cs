using Godot;
using Godot.Collections;

namespace OrbinautFramework3.Objects.Common.FallingFloor;

[Tool]
public partial class FallingFloorSpawner : Sprite2D
{
    [Export] private Vector2I CollisionRadius
    {
        get => _collisionRadius;
        set
        {
            _collisionRadius = value;
#if TOOLS
            QueueRedraw();
#endif
        }
    }

    [Export] private Vector2I _piecesSize = new(16, 16);
    [Export] private Array<AtlasTexture> _piecesTextures;

    private Vector2I _collisionRadius;
    
    public FallingFloorSpawner() => TextureChanged += UpdateConfigurationWarnings;

    public override void _Ready()
    {
#if TOOLS
        if (Engine.IsEditorHint())
        {
            TextureChanged += CreatePiecesTextures;
            if (_piecesTextures != null) return;
            CreatePiecesTextures();
            return;
        }
#endif
        SpawnFloor();
        QueueFree();
    }

#if TOOLS
    public override void _Draw()
    {
        base._Draw();
        DrawRect(new Rect2(-_collisionRadius, _collisionRadius * 2), 
            new Color(Colors.Yellow, 0.5f), false, 1f);
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (property != "scale") return base._Set(property, value);
        Scale = new Vector2(((Vector2)value).X >= 0f ? 1f : -1f, 1f);
        return true;
    }

    public override string[] _GetConfigurationWarnings() => Texture == null ? ["Please set `Texture`."] : [];

    private void CreatePiecesTextures()
    {
        if (Texture == null)
        {
            _piecesTextures = null;
            return;
        }

        _piecesTextures = [];
        var size = (Vector2I)Texture.GetSize();

        for (var x = 0; x < size.X; x += _piecesSize.X)
        for (var y = 0; y < size.Y; y += _piecesSize.Y)
        {
            _piecesTextures.Add(new AtlasTexture
            {
                Atlas = Texture,
                Region = new Rect2(x, y, _piecesSize)
            });
        }
    }
#endif
    
    private void SpawnFloor()
    {
        if (Texture == null) return;
        
        Node parent = GetParent();
        if (parent == null) return;

        var sprite = new Sprite2D
        {
            Texture = Texture,
            Offset = Offset
        };
        
        var floor = new FallingFloor(sprite, _piecesTextures, _piecesSize)
        {
            Position = Position,
            ProcessPriority = ProcessPriority,
            ZIndex = ZIndex
        };
        
        floor.AddChild(sprite);
        floor.SetSolid(CollisionRadius);
        
        parent.CallDeferred("add_child", floor);
    }
}