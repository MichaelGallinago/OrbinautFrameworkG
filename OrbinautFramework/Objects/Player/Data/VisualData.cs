using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public float DustTimer { get; set; }
    public int? OverrideFrame { get; set; }
    public Animations Animation { get; set; }
    public object SetPushBy { get; set; } //TODO: replace OrbinautNode with interface
    public Constants.Direction Facing { get; set; }

    public void Init()
    {
        Facing = Constants.Direction.Positive;
        Animation = Animations.Idle;
        DustTimer = 0f;
        SetPushBy = null;
        OverrideFrame = null;
    }
}
