using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;

namespace OrbinautFrameworkG.Framework.View;

public interface ICamera
{
    public Vector2I DrawPosition { get; }
    public Vector2 PreviousPosition { get; }
    public int BoundSpeed { get; set; }
    public Vector4 TargetBoundary { get; set; }
    public Vector4 Boundary { get; }
    public IPosition Target { set; }
    public bool IsMovementAllowed { get; set; }
    public bool IsMoved { get; }
    
    public bool CheckRectInside(Rect2 rect);
    public bool CheckPositionInSafeRegion(Vector2I position);
    public bool CheckPositionInActiveRegion(Vector2I position);
    public void SetShakeTimer(float shakeTimer);
    public void SetCameraDelayX(float delay);
}
