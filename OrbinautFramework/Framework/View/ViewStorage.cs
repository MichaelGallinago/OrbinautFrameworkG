using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OrbinautFramework3.Framework.View;

public partial class ViewStorage : Control
{
    [Export] private PackedScene _packedView;
    [Export] private byte _number;

    private readonly List<CameraView> _views = [];
    private readonly List<Camera> _cameras = [];
    
    public override void _Ready()
    {
        for (var i = 0; i < _number; i++)
        {
            CreateView();
        }
    }

    public bool CheckRectInCamera(int index)
    {
        return index < _cameras.Count && _cameras[index].CheckRectInside();
    }
    
    public bool CheckRectInCameras()
    {
        return _cameras.Any(camera => camera.CheckRectInside());
    }

    private void CreateView()
    {
        var view = _packedView.Instantiate<CameraView>();
        _cameras.Add(view.Camera);
        _views.Add(view);
    }

    //TODO: Replace to interface and move to Scene
    public static Camera Main { get; set; }
}