using System;
using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.View;

public partial class ViewStorage : Control
{
    public static ViewStorage Local => FrameworkData.CurrentScene.ViewStorage;
    
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

    public bool CheckRectInCamera(Rect2 rect, int index)
    {
        return index < _cameras.Count && _cameras[index].CheckRectInside(rect);
    }
    
    public bool CheckRectInCameras(Rect2 rect)
    {
        foreach (Camera camera in _cameras)
        {
            if (camera.CheckRectInside(rect)) return true;
        }

        return false;
    }

    public void FillStackOfWithResumingRegions(Stack<Camera> stack)
    {
        foreach (Camera camera in _cameras)
        {
            stack.Push(camera);
        }
    }
    
    public void InvokeInCameras(Action<Camera> action)
    {
        foreach (Camera camera in _cameras)
        {
            action(camera);
        }
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