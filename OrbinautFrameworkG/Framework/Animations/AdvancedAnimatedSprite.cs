using System;
using Godot;
using OrbinautFrameworkG.Framework.View;

namespace OrbinautFrameworkG.Framework.Animations;

[Tool, GlobalClass]
public partial class AdvancedAnimatedSprite : AnimatedSprite2D
{
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

    private StringName _nextAnimation;
    private int _frameLoop;
    private AdvancedSpriteFrames _advancedSpriteFrames;
    
    public override void _Ready()
    {
        AnimationLooped += LoopFrame;

        RegisterAdvancedSpriteFrames();
        SpriteFramesChanged += UpdateSpriteFrames;
        UpdateSpriteFrames();

        Position = default;
#if TOOLS
        if (Engine.IsEditorHint() && Owner.Owner == null) return;
#endif
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
        
        NotifyPropertyListChanged();
    }
#endif
    
    public bool CheckInCamera(ICamera camera) => camera.CheckRectInside(CullRect);
    public bool CheckInCameras() => Views.Instance.CheckRectInCameras(CullRect);

    public void PlayAnimation(StringName animation)
    {
        if (Animation == animation) return;
        Play(animation);
    }
    
    public void PlayAnimation(StringName animation, float speedScale)
    {
        SpeedScale = speedScale;
        PlayAnimation(animation);
    }
    
    public void PlayAnimation(StringName animation, int startFrame)
    {
        if (Animation == animation) return;
        Play(animation);
        Frame = startFrame;
    }
    
    public void PlayAnimation(StringName animation, int startFrame, float speedScale)
    {
        SpeedScale = speedScale;
        PlayAnimation(animation, startFrame);
    }
    
    private void UpdateSpriteFrames()
    {
        if (_advancedSpriteFrames != null)
        {
            AnimationChanged -= UpdateValues;
#if TOOLS
            if (Engine.IsEditorHint())
            {
                AnimationChanged -= _advancedSpriteFrames.Refresh;
            }
#endif
        }
        
        _advancedSpriteFrames = SpriteFrames as AdvancedSpriteFrames;
        RegisterAdvancedSpriteFrames();
    }

    private void RegisterAdvancedSpriteFrames()
    {
        if (_advancedSpriteFrames == null) return;
        
        AnimationChanged += UpdateValues;
        UpdateValues();
            
#if TOOLS
        if (!Engine.IsEditorHint()) return;
        AnimationChanged += _advancedSpriteFrames.Refresh;
        _advancedSpriteFrames.Refresh();
#endif
    }

    private void LoopFrame() => Frame = _frameLoop;
    
    private void UpdateValues()
    {
        _frameLoop = _advancedSpriteFrames.GetAnimationFrameLoop(Animation);
        Offset = _advancedSpriteFrames.GetAnimationOffset(Animation);
    }

    private Rect2 CullRect => new(Position + Offset, _advancedSpriteFrames.CullSize * Scale);
}
