using Godot;
using OrbinautFramework3.Scenes;

namespace OrbinautFramework3.Framework.Background;

public partial class BackgroundLayer : Sprite2D
{
    //private static PackedScene _packedShader = ResourceLoader.Load<PackedScene>();
    
    [Export] private int _animationDuration;
    [Export] private Vector2 _factor;
    [Export] private Vector2 _scroll;
    [Export] private int _inclineHeight;
    [Export] private float _inclineFactor;
    [Export] private int _scaleTarget;
    
    private Vector2I _offset;
    private Vector2 _shift;
    private int _height;

    public override void _Ready() => _height = RegionEnabled ? (int)RegionRect.Size.Y : Texture.GetHeight();

    public override void _Process(double delta)
    {
        _shift += _scroll * Scene.Speed;
    }
}
