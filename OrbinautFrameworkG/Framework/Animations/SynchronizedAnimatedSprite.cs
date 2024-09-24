using Godot;

namespace OrbinautFrameworkG.Framework.Animations;

public partial class SynchronizedAnimatedSprite : AnimatedSprite2D
{
    public float Duration { get; set; } = 1f;
        
    private int _frameCount;

    public SynchronizedAnimatedSprite()
    {
        AnimationChanged += UpdateFrameCount;
        SpriteFramesChanged += UpdateFrameCount;
        UpdateFrameCount();
    }

    public SynchronizedAnimatedSprite(float duration = 1f) : this() => Duration = duration;

    public void AnimateSynchronously(float timer)
    {
        Frame = (int)(timer / Duration) % _frameCount;
    }
    
    private void UpdateFrameCount() => _frameCount = SpriteFrames.GetFrameCount(Animation);
}
