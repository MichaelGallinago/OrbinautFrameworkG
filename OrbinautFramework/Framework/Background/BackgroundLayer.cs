using Godot;

namespace OrbinautFramework3.Framework.Background;

public partial class BackgroundLayer : Sprite2D
{
    //private static PackedScene _packedShader = ResourceLoader.Load<PackedScene>();
    
    [Export] public int AnimationDuration;
    [Export] public Vector2 Factor;
    [Export] public Vector2 Scroll;
    [Export] public int InclineHeight;
    [Export] public float InclineFactor;
    [Export] public int ScaleTarget;
    
    private Vector2I _offset;
    private Vector2 _shift;
    private int _height;

    public override void _Ready() => _height = RegionEnabled ? (int)RegionRect.Size.Y : Texture.GetHeight();

    public override void _Process(double delta)
    {
        _shift += Scroll * Scene.Instance.Speed;
    }
}