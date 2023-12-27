using System;
using Godot;

namespace OrbinautFramework3.Framework.Animations;

public partial class AdvancedAnimatedSprite : AnimatedSprite2D
{
    [Export] public int FrameLoop 
    { 
        get => _frameLoop;
        set
        {
            _frameLoop = Math.Clamp(value, 0, SpriteFrames.GetFrameCount(Animation));
            switch (_isCustomLoop)
            {
                case true when _frameLoop == 0:
                    _isCustomLoop = false;
                    AnimationLooped -= LoopFrame;
                    break;
                case false when _frameLoop != 0:
                    _isCustomLoop = true;
                    AnimationLooped += LoopFrame;
                    break;
            }
        }
    }
    
    public StringName NextAnimation { get; set; }
    
    private bool _isCustomLoop;
    private int _frameLoop;
    
    public AdvancedAnimatedSprite()
    {
        AnimationChanged += () => FrameLoop = 0;
        AnimationFinished += () =>
        {
            if (NextAnimation == null) return;
            Animation = NextAnimation;
        };
    }
    
    private void LoopFrame() => Frame = _frameLoop;
}
