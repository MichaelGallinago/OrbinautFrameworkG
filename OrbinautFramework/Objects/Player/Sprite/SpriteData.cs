namespace OrbinautFramework3.Objects.Player.Sprite;

public class SpriteData
{
    public float Speed { get; set; }
    public int FrameCount { get; set; }
    public bool IsFinished { get; set; }
    public Animations Type { get; set; }
    public bool IsFrameChanged { get; set; }
    public Animations Animation { get; set; }
}
