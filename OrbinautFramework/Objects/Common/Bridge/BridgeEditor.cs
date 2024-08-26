using Godot;

namespace OrbinautFramework3.Objects.Common.Bridge;

[Tool]
public partial class BridgeEditor : Node2D
{
    [Export] public Texture2D LogTexture 
    {
        get => _logTexture;
        private set
        {
            _logTexture = value;
            UpdateConfigurationWarnings();
            QueueRedraw();
        }
    }
    private Texture2D _logTexture;

    [Export] public byte LogAmount
    {
        get => _logAmount;
        private set
        {
            _logAmount = value;
            QueueRedraw();
        }
    }
    private byte _logAmount = 8;

    [Export] public byte LogWidth
    {
        get => _logWidth;
        private set 
        {
            _logWidth = value;
            QueueRedraw();
        }
    }
    private byte _logWidth = 16;
    
    public override void _Draw()
    {
        if (_logTexture == null) return;
        
        var x = 0;
        int y = _logWidth / -2;
        for (var i = 0; i < _logAmount; i++)
        {
            DrawTexture(_logTexture, new Vector2(x, y));
            x += _logWidth;
        }
    }
}
