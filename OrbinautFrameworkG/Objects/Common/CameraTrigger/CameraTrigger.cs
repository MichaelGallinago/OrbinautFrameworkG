using System;
using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.View;
using static OrbinautFrameworkG.Framework.Constants;

namespace OrbinautFrameworkG.Objects.Common.CameraTrigger;

public partial class CameraTrigger : Trigger
{
    private enum BoundSpeeds : byte { Speed2 = 2, Speed4 = 4, Speed8 = 8, Speed16 = 16, Speed32 = 32 }
    
    [Export] private Sprite2D _sprite;
    [Export] private BoundSpeeds _boundSpeed;
    [Export] private Direction _direction;
    [Export] private bool _triggerWithin;
    
    private int _height;
    private Vector2I[] _previousBounds;

    public override void _Ready()
    {
        if (_sprite?.Texture == null) return;
        _height = (int)(_sprite.Texture.GetSize().Y * Scale.Y) / 2;
        
        Views.Instance.OnViewNumberChanged += CreateTargetBoundStorages;
        CreateTargetBoundStorages(Views.Instance.Number);
    }

    private void CreateTargetBoundStorages(int number)
    {
        int i;
        var newArray = new Vector2I[number];
        if (_previousBounds != null)
        {
            i = _previousBounds.Length;
            Array.Copy(_previousBounds, newArray, i > number ? number : i);
            _previousBounds = newArray;
        }
        else
        {
            i = 0;
            _previousBounds = new Vector2I[number];
        }
        
        int length = Views.Instance.Cameras.Length;
        var defaultBound = new Vector2I(0, ushort.MaxValue);
        for (; i < length; i++)
        {
            _previousBounds[i] = defaultBound;
        }
    }

    public override void _Process(double delta)
    {
        Vector2I halfSize = Settings.ViewSize / 2;
        ReadOnlySpan<ICamera> cameras = Views.Instance.Cameras;
        var triggerBounds = new Vector2I((int)Position.Y - _height, (int)Position.Y + _height);
        
        for (var i = 0; i < cameras.Length; i++)
        {
            ICamera camera = cameras[i];
            Vector2I position = camera.DrawPosition + halfSize;

            float previousX = camera.PreviousPosition.X + halfSize.X;
	
            if (_triggerWithin && (position.Y < triggerBounds.X || position.Y >= triggerBounds.Y)) continue;

            if (position.X >= Position.X == previousX < Position.X) continue;
            
            if (position.X >= Position.X == (_direction == Direction.Positive))
            {
                SetNewBound(i, camera, triggerBounds);
            }
            else
            {
                SetPreviousBound(i, camera);
            }
        }
    }

    private void SetNewBound(int viewIndex, ICamera camera, Vector2I triggerBounds)
    {
        Vector4 targetBoundary = camera.TargetBoundary;
        _previousBounds[viewIndex] = new Vector2I((int)targetBoundary.Y, (int)targetBoundary.W);
        
        camera.BoundSpeed = (int)_boundSpeed;
        camera.TargetBoundary = targetBoundary with { Y = triggerBounds.X, W = triggerBounds.Y };
    }

    private void SetPreviousBound(int viewIndex, ICamera camera)
    {
        camera.BoundSpeed = (int)_boundSpeed;
        Vector2I bound = _previousBounds[viewIndex];
        camera.TargetBoundary = camera.TargetBoundary with { Y = bound.X, W = bound.Y };
    }
}
