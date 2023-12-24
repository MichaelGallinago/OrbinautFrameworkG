using Godot;

namespace OrbinautFramework3.Objects.Common.Bridge;

[Tool]
public partial class BridgeSpawner : Node2D
{
    [Export] private Texture2D LogTexture 
    {
        get => _logTexture;
        set
        {
            _logTexture = value;
            UpdateConfigurationWarnings();
        }
    }
    
    [Export] private byte _logCount = 8;
    [Export] private short _logOffset;
    [Export] private Texture2D _stampTexture;

    private Texture2D _logTexture;
    private Vector2 _stampSize;
    private int _logSeparation;

    public override void _Ready()
    {
        if (LogTexture == null)
        {
            if (Engine.IsEditorHint()) return;
            QueueFree();
            return;
        }

        if (_stampTexture != null)
        {
            _stampSize = _stampTexture.GetSize();
        }
        
        _logSeparation = (int)LogTexture.GetSize().X + _logOffset;
        
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
            return;
        }

        SpawnBridge();
        QueueFree();
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Engine.IsEditorHint()) return;
        
        int length = _logSeparation * _logCount;
        for (var drawX = 0; drawX < length; drawX += _logSeparation)
        {
            DrawTexture(LogTexture, Position + new Vector2(drawX, 0f));
        }

        if (_stampTexture == null) return;
        DrawTexture(_stampTexture, Position - _stampSize);
        DrawTexture(_stampTexture, Position + new Vector2(length, -_stampSize.Y));
    }

    public override string[] _GetConfigurationWarnings() => _logTexture == null ? ["Please set `Log Texture`."] : [];

    private void SpawnBridge()
    {
        Node parent = GetParent();
        if (parent == null) return;
        GD.Print("aboba");
        var bridge = new Bridge(LogTexture, _logCount, _logSeparation);
        bridge.Position = Position;
        parent.CallDeferred("add_child", bridge);
        
        if (_stampTexture == null) return;
        
        SpawnStamp(parent, -_stampSize);
        SpawnStamp(parent, new Vector2(_logCount * _logOffset, -_stampSize.Y));
    }

    private void SpawnStamp(GodotObject parent, Vector2 offset)
    {
        //TODO: depth
        var stamp = new Sprite2D();
        stamp.Texture = _stampTexture;
        stamp.Position = Position + offset;
        parent.CallDeferred("add_child", stamp);
    }
}