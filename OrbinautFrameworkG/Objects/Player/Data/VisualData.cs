using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Data;

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

    public void Init(Vector2 scale, int zIndex)
    {
        Scale = scale;
        ZIndex = zIndex;
        
        Angle = 0f;
        Facing = Constants.Direction.Positive;
        Visible = true;
        DustTimer = 0f;
        SetPushBy = null;
        OverrideFrame = null;
    }
}
