using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public int? OverrideFrame { get; set; }
    public bool IsFrameChanged { get; set; }
    public Animations Animation { get; set; }
    public OrbinautNode SetPushBy { get; set; } //TODO: replace OrbinautNode with interface
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
