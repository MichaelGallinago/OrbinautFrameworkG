using Godot;
using OrbinautFramework3.Framework;
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
        
        _previousBound = Camera.Main.LimitBottom;
        _cameraSide = Camera.Main.BufferPosition.X + SharedData.ViewSize.X / 2f < Position.X ? 
            Direction.Positive : Direction.Negative;
    }

    public override void _Process(double delta)
    {
        Camera camera = Camera.Main;
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
