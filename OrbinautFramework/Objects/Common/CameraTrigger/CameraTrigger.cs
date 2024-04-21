using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using static OrbinautFramework3.Framework.Constants;

namespace OrbinautFramework3.Objects.Common.CameraTrigger;

public partial class CameraTrigger : Trigger
{
    private enum BoundSpeeds : byte
    {
        Speed2 = 2, Speed4 = 4, Speed8 = 8, Speed16 = 16, Speed32 = 32
    }
    
    [Export] private Sprite2D _sprite;
    [Export] private int _newBoundY = -1;
    [Export] private BoundSpeeds _boundSpeed;
    [Export] private Direction _direction;
    
    private int _height;
    private int _previousBound;
    private Direction _cameraSide;

    public override void _Ready()
    {
        if (_sprite?.Texture == null) return;
        _height = (int)(_sprite.Texture.GetSize().Y * Scale.Y) / 2;
        
        _previousBound = Framework.Camera.Main.LimitBottom;
        _cameraSide = Framework.Camera.Main.BufferPosition.X + SharedData.ViewSize.X / 2f < Position.X ? 
            Direction.Positive : Direction.Negative;
    }

    public override void _Process(double delta)
    {
        foreach (var VARIABLE in Views.Local.Cameras)
        {
            
        }
        Framework.Camera camera = Framework.Camera.Main;
        Vector2I position = camera.BufferPosition + SharedData.ViewSize / 2;
	    
        if (position.Y < Position.Y - _height || position.Y >= Position.Y + _height) return;
        
        if (_cameraSide == Direction.Positive == position.X < Position.X) return;
        
        _cameraSide = (Direction)(-(int)_cameraSide);
        camera.BoundSpeed.Y = (int)_boundSpeed;
	    
        if (_direction == Direction.Negative && position.X < Position.X || 
            _direction == Direction.Positive && position.X >= Position.X)
        {
            _previousBound = camera.Bounds.W;
            camera.Bounds.W = _newBoundY > -1 ? _newBoundY : camera.LimitBottom;
        }
        else
        {
            camera.Bounds.W = _previousBound;
        }
    }
}
