using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public int ZIndex { get; set; }
    public float Angle { get; set; }
    public bool Visible { get; set; }
    public Vector2 Scale { get; set; }
    public float DustTimer { get; set; }
    public object SetPushBy { get; set; }
    public int? OverrideFrame { get; set; }
    public Constants.Direction Facing { get; set; }

    public void Init()
    {
        Angle = 0f;
        Facing = Constants.Direction.Positive;
        DustTimer = 0f;
        SetPushBy = null;
        OverrideFrame = null;
    }
}
