using Godot;
using OrbinautFramework3.Framework;

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

    [Export] private byte LogAmount
    {
        get => _logAmount;
        set
        {
            _logAmount = value;
            UpdateLength();
        }
    }

    [Export] private short LogOffset
    {
        get => _logOffset;
        set 
        {
            _logOffset = value;
            UpdateLength();
        }
    }

    [Export] private Texture2D _stampTexture;
    [Export] private ICullable.Types _cullingType;
    
    private byte _logAmount = 8;
    private short _logOffset;
    
    private Texture2D _logTexture;
    private Vector2 _stampSize;
    private Vector2I _logSize;
    private int _length;

    public override void _Ready()
    {
        if (LogTexture == null)
        {
#if TOOLS
            if (Engine.IsEditorHint()) return;
#endif
            
            QueueFree();
            return;
        }

        if (_stampTexture != null)
        {
            _stampSize = _stampTexture.GetSize();
        }

        UpdateLength();
        
#if TOOLS
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
            return;
        }
#endif

        SpawnBridge();
        QueueFree();
    }
    
#if TOOLS
    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Engine.IsEditorHint()) return;
        
        for (var drawX = 0; drawX < _length; drawX += _logSize.X)
        {
            DrawTexture(LogTexture,  new Vector2(drawX, -_logSize.Y / 2f));
        }

        if (_stampTexture == null) return;
        float stampY = -_stampSize.Y * 1.5f;
        DrawTexture(_stampTexture, new Vector2(-_stampSize.X, stampY));
        DrawTexture(_stampTexture, new Vector2(_length, stampY));
    }

    public override string[] _GetConfigurationWarnings() => _logTexture == null ? ["Please set `Log Texture`."] : [];
#endif
    
    private void UpdateLength()
    {
        _logSize = (Vector2I)LogTexture.GetSize();
        _logSize.X += _logOffset;
        _length = _logSize.X * _logAmount;
    }

    private void SpawnBridge()
    {
        Node parent = GetParent();
        if (parent == null) return;

        var bridge = new Bridge(LogTexture, _logAmount, _logSize.X)
        {
            Position = Position + new Vector2((_length - _logSize.X) / 2f, 0f),
            ProcessPriority = ProcessPriority,
            ZIndex = ZIndex,
        };
        bridge.CullingType = _cullingType;
        parent.CallDeferred("add_child", bridge); //TODO: replace to AddChild() somehow
        
        if (_stampTexture == null) return;

        float stampY = (_stampSize.Y + _logSize.Y) / -2f;
        SpawnStamp(parent, new Vector2(-_stampSize.X / 2f, stampY));
        SpawnStamp(parent, new Vector2(_length + _stampSize.X / 2f, stampY));
    }

    private void SpawnStamp(GodotObject parent, Vector2 offset)
    {
        var stamp = new Sprite2D
        {
            Texture = _stampTexture, 
            Position = Position + offset
        };
        parent.CallDeferred("add_child", stamp); //TODO: replace to AddChild() somehow
    }
}
