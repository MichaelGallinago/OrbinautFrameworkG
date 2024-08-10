using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

public class VisualData
{
    public float Angle { get; set; }
    public float DustTimer { get; set; }
    public object SetPushBy { get; set; } //TODO: replace OrbinautNode with interface
    public int? OverrideFrame { get; set; }
    public Animations Animation { get; set; }
    public Constants.Direction Facing { get; set; }

    public void Init()
    {
        Angle = 0f;
        Facing = Constants.Direction.Positive;
        Animation = Animations.Idle;
        DustTimer = 0f;
        SetPushBy = null;
        OverrideFrame = null;
    }
}
