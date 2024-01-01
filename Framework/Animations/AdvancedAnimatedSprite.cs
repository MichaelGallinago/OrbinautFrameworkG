using System;
using Godot;

namespace OrbinautFramework3.Framework.Animations;

[Tool, GlobalClass]
public partial class AdvancedAnimatedSprite : AnimatedSprite2D
{
    public StringName NextAnimation { get; set; }
    
    [Export] private int FrameLoop
    {
        get => _frameLoop;
        set
        {
            _frameLoop = Math.Clamp(value, 0, SpriteFrames.GetFrameCount(Animation));
            if (_frameLoop == 0)
            {
                _advancedSpriteFrames?.RemoveAnimationFrameLoop(Animation);
                return;
            }

            _advancedSpriteFrames?.SetAnimationFrameLoop(Animation, _frameLoop);
        }
    }
    
    private int _frameLoop;
    private AdvancedSpriteFrames _advancedSpriteFrames;
    
    public override void _Ready()
    {
        AnimationFinished += SetNextAnimation;
        AnimationLooped += LoopFrame;
        SpriteFramesChanged += UpdateSpriteFrames;
        UpdateSpriteFrames();

        Position = default;
        SetProcess(false);
    }
    
#if TOOLS
    public override void _Process(double delta)
    {
        if (_advancedSpriteFrames == null) return;
        Vector2 lastOffset = _advancedSpriteFrames.GetAnimationOffset(Animation);
        
        Position = -Offset;
        
        if (lastOffset == Offset) return;
        
        if (Offset == default)
        {
            _advancedSpriteFrames.RemoveAnimationOffset(Animation);
            return;
        }
        
        _advancedSpriteFrames.SetAnimationOffset(Animation, Offset);
    }
#endif

    private void UpdateSpriteFrames()
    {
        _advancedSpriteFrames = SpriteFrames as AdvancedSpriteFrames;

        AnimationChanged -= UpdateValues;
        if (_advancedSpriteFrames == null) return;
        AnimationChanged += UpdateValues;
            
        #if TOOLS
        if (Engine.IsEditorHint())
        {
            AnimationChanged += _advancedSpriteFrames.Refresh;
        }
        #endif
    }

    private void LoopFrame()
    {
        if (_frameLoop == 0) return;
        Frame = _frameLoop;
    }

    private void UpdateValues()
    {
        _frameLoop = _advancedSpriteFrames.GetAnimationFrameLoop(Animation);
        Offset = _advancedSpriteFrames.GetAnimationOffset(Animation);
    }
    
    private void SetNextAnimation()
    {
        if (NextAnimation == null) return;
        Animation = NextAnimation;
    }
    
    public bool CheckInView()
    {
        Vector4I bounds = Camera.Main.Bounds;
        Vector2 size = SpriteFrames.GetFrameTexture(Animation, Frame).GetSize();
        
        return Position.X >= bounds.X - size.X && Position.X <= bounds.Z + size.X &&
               Position.Y >= bounds.Y - size.Y && Position.Y <= bounds.W + size.Y;
    }

    public void SetAnimation(StringName animation, float customSpeed = 1f)
    {
        if (Animation == animation) return;
        Play(animation, customSpeed);
    }
    
    public void SetAnimation(StringName animation, int startFrame, float customSpeed = 1f)
    {
        if (Animation == animation) return;
        Play(animation, customSpeed);
        Frame = startFrame;
    }
}
