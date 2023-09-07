using Godot;
using System;

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

    public override void _Ready()
    {
        if (RegionEnabled)
        {
            _height = (int)RegionRect.Size.Y;
        }
        else
        {
            _height = Texture.GetHeight();
        }
    }

    public override void _EnterTree()
    {
        FrameworkData.CurrentScene.LateUpdate += EndStep;
    }

    public override void _ExitTree()
    {
        FrameworkData.CurrentScene.LateUpdate -= EndStep;
    }

    private void EndStep(double processSpeed)
    {
        if (!FrameworkData.UpdateGraphics) return;
        _shift += Scroll * (float)processSpeed;
    }
}
