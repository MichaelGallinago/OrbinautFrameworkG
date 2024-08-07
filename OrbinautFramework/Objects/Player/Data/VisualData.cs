using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public Animations Type { get; set; }
    public bool IsFrameChanged { get; set; }
    public int? OverrideFrame { get; set; }
    public OrbinautData SetPushBy { get; set; }
    public Constants.Direction Facing { get; set; }

    public void Init()
    {
        Type = Animations.Idle;
        Facing = Constants.Direction.Positive;
        SetPushBy = null;
        OverrideFrame = null;
        IsFrameChanged = false;
    }
}
