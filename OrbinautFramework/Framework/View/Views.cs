using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Scenes;

namespace OrbinautFramework3.Framework.View;

public partial class Views : Control, IViews
{
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
    
    public ICamera BottomCamera { get; private set; }
    public ReadOnlySpan<ICamera> Cameras => _cameras;
    public Dictionary<BaseObject, ICamera> TargetedCameras { get; } = [];
    
    [Export] private byte _number = 1;
    [Export] private PackedScene _packedViewContainer;
    
    [UsedImplicitly] private IScene _scene;
    
    private Camera[] _cameras;
    private ViewContainer[] _containers;
    private readonly List<ICamera> _camerasWithUpdatedRegions = [];

    public override void _Ready()
    {
        CreateViews();
        AttachCamerasToPlayers();
        
        // TODO: check for memory leak
        SharedData.ViewSizeChanged += OnViewSizeChanged;
        OnViewSizeChanged(SharedData.ViewSize);
    }

    private void OnViewSizeChanged(Vector2I viewSize)
    {
        Vector2I renderBuffer = Constants.RenderBuffer * Vector2I.Right;
        Vector2I size = viewSize + 2 * renderBuffer;
        Position = -renderBuffer;
        Size = size;

        foreach (ViewContainer container in _containers)
        {
            container.SubViewport.Size = size;
            container.Size = Size;
        }
    }

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
        
        if (camera.DrawPosition.Y > BottomCamera.DrawPosition.Y)
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
    
    private void AttachCamerasToPlayers()
    {
        ReadOnlySpan<Player> players = _scene.Players.Values;
        int number = Math.Min(_cameras.Length, players.Length);
        for (var i = 0; i < number; i++)
        {
            _cameras[i].Target = players[i];
        }
    }

    private void RemoveViews()
    {
        Godot.Collections.Array<Node> children = GetChildren();
        for (var i = 0; i < children.Count; i++)
        {
            children[i].QueueFree();
        }
    }

    private void CreateViews()
    {
        if (_number == 0) return;
        int columnsCount = GetColumnsCount(_number);
        
        var boxContainer = new HBoxContainer();
        AddChild(boxContainer);

        _cameras = new Camera[_number];
        _containers = new ViewContainer[_number];
        
        var column = 0;
        for (var i = 0; i < _number; i++)
        {
            if (++column > columnsCount)
            {
                column = 0;
                AddChild(boxContainer = new HBoxContainer());
            }
            
            var viewContainer = _packedViewContainer.Instantiate<ViewContainer>();
            _cameras[i] = viewContainer.Camera;
            _containers[i] = viewContainer;
            viewContainer.SubViewport.SetWorld2D(_scene.GetWorld2D());
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
