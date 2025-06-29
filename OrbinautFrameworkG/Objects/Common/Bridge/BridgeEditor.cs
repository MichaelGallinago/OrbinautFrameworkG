﻿using Godot;

namespace OrbinautFrameworkG.Objects.Common.Bridge;

[Tool]
public partial class BridgeEditor : Node2D
{
#if TOOLS
    [Export] public Texture2D LogTexture 
    {
        get => _logTexture;
        private set
        {
            _logTexture = value;
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
    
    private int _halfWidth;
    
    public BridgeEditor()
    {
        int logHalfWidth = _logWidth / 2;
        _halfWidth = _logAmount * logHalfWidth - logHalfWidth;
    }
    
    public override void _Draw()
    {
        if (_logTexture == null) return;
        
        int x = -_halfWidth;
        int y = _logWidth / -2;
        for (var i = 0; i < _logAmount; i++)
        {
            DrawTexture(_logTexture, new Vector2(x, y));
            x += _logWidth;
        }
    }
#else
    [Export] public Texture2D LogTexture { get; private set; }
    [Export] public byte LogAmount { get; private set; }
    [Export] public byte LogWidth { get; private set; }
#endif
}
