namespace OrbinautFramework3.Objects.Player.Sprite;

public class SpriteData(ISpriteNode node)
{
    public ISpriteNode Node { get; private set; } = node;
    
    public float Speed { get; set; }
    public int FrameCount { get; set; }
    public bool IsFinished { get; set; }
    public Animations Type { get; set; }
    public bool IsFrameChanged { get; set; }
}
