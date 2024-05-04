using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework.View;

public partial class Views : Control
{
    public static Views Local => Scene.Local.Views;

    public event Action<int> OnViewNumberChanged;
    
    public byte Number
    { 
        get => _number;
        set
        {
            _number = value;
            CreateViews();
            OnViewNumberChanged?.Invoke(_number);
        }
    }
    
    [Export] private byte _number = 1;
    [Export] private PackedScene _packedViewContainer;
    [Export] private VBoxContainer _boxContainer;
    
    public ReadOnlySpan<ICamera> Cameras => _cameras;
    public Dictionary<BaseObject, ICamera> TargetedCameras { get; } = [];
    public ICamera BottomCamera { get; private set; }
    private Camera[] _cameras;
    
    private ViewContainer[] _containers;
    private readonly List<ICamera> _camerasWithUpdatedRegions = [];

    public override void _Ready() => CreateViews();
    
    public bool CheckRectInCameras(Rect2 rect)
    {
        foreach (Camera camera in _cameras)
        {
            if (camera.CheckRectInside(rect)) return true;
        }

        return false;
    }

    public void UpdateBottomCamera(ICamera camera)
    {
        if (BottomCamera == null)
        {
            BottomCamera = camera;
            return;
        }
        
        if (camera.BufferPosition.Y > BottomCamera.BufferPosition.Y)
        {
            BottomCamera = camera;
        }
    }

    public ReadOnlySpan<ICamera> GetCamerasWithUpdatedRegions()
    {
        _camerasWithUpdatedRegions.Clear();
        
        foreach (Camera camera in _cameras)
        {
            if (camera.IsActiveRegionChanged)
            {
                _camerasWithUpdatedRegions.Add(camera);
            }
        }
        
        return CollectionsMarshal.AsSpan(_camerasWithUpdatedRegions);
    }

    private void RemoveViews()
    {
        _boxContainer.QueueFree();
        AddChild(_boxContainer = new VBoxContainer());
    }

    private void CreateViews()
    {
        if (_number == 0) return;
        int columnsCount = GetColumnsCount(_number);
        
        var boxContainer = new HBoxContainer();
        _boxContainer.AddChild(boxContainer);

        _cameras = new Camera[_number];
        _containers = new ViewContainer[_number];
        
        var column = 0;
        for (var i = 0; i < _number; i++)
        {
            if (++column > columnsCount)
            {
                column = 0;
                _boxContainer.AddChild(boxContainer = new HBoxContainer());
            }
            
            var viewContainer = _packedViewContainer.Instantiate<ViewContainer>();
            _cameras[i] = viewContainer.Camera;
            _containers[i] = viewContainer;
            boxContainer.AddChild(viewContainer);
        }
    }
    
    private static int GetRowsCount(int viewNumber)
    {
        return Mathf.CeilToInt((MathF.Sqrt(1 + 4 * viewNumber) - 1f) / 2f);
    }

    private static int GetColumnsCount(int viewNumber)
    {
        return Mathf.CeilToInt(viewNumber / (float)GetRowsCount(viewNumber));
    }
}
