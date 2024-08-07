using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public int? OverrideFrame { get; set; }
    public bool IsFrameChanged { get; set; }
    public Animations Animation { get; set; }
    public OrbinautData SetPushBy { get; set; }
    public Constants.Direction Facing { get; set; }

    public void Init()
    {
        Facing = Constants.Direction.Positive;
        Animation = Animations.Idle;
        SetPushBy = null;
        OverrideFrame = null;
        IsFrameChanged = false;
    }
}
