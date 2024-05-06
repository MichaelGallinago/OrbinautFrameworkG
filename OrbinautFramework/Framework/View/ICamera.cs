using Godot;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework.View;

public interface ICamera
{
    public Vector2I DrawPosition { get; }
    public Vector2I PreviousPosition { get; }
    public int BoundSpeed { get; set; }
    public Vector4 TargetBoundary { get; set; }
    public Vector4 Boundary { get; }
    public BaseObject Target { set; }
    public bool IsMovementAllowed { get; set; }
    
    public bool CheckRectInside(Rect2 rect);
    public bool CheckPositionInSafeRegion(Vector2I position);
    public bool CheckPositionInActiveRegion(Vector2I position);
    public bool CheckXInActiveRegion(int position);
    public bool CheckYInActiveRegion(int position);
    public void SetShakeTimer(float shakeTimer);
    public void SetCameraDelayX(float delay);
}
