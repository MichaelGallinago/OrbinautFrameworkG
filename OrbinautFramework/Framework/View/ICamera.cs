using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework.View;

public interface ICamera
{
    public Vector2I BufferPosition { get; }
    public Vector2I PreviousPosition { get; }
    public Vector2 Delay { get; set; }
    public int BoundSpeed { get; set; }
    public Vector4 Bound { get; set; }
    public Vector4 Limit { get; }
    public BaseObject Target { set; }
    public Vector2 Position { get; set; }
    public bool IsMovementAllowed { get; set; }
    
    public bool CheckRectInside(Rect2 rect);
    public bool CheckPositionInSafeRegion(Vector2I position);
    public bool CheckPositionInActiveRegion(Vector2I position);
    public bool CheckXInActiveRegion(int position);
    public bool CheckYInActiveRegion(int position);
}
