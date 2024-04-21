using Godot;

namespace OrbinautFramework3.Framework.View;

public interface ICamera
{
    public Rect2I ActiveRegion { get; }
    public Vector2I BufferPosition { get; }
    public Vector2 Delay { get; }
    public Vector2I BoundSpeed { get; }
    public Vector4 Bound { get; }
    public Vector4 Limit { get; }
    public Vector2 Position { get; }
    
    public bool CheckRectInside(Rect2 rect);
    public bool CheckPositionInSafeRegion(Vector2I position);
    public bool CheckPositionInActiveRegion(Vector2I position);
    public bool CheckXInActiveRegion(int position);
    public bool CheckYInActiveRegion(int position);
}