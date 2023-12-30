using System;
using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.Animations;

[Tool]
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
                _frameLoops.Remove(Animation);
                return;
            }

            if (_frameLoops.TryAdd(Animation, value)) return;
            _frameLoops[Animation] = value;
        }
    }

    [Export] private Vector2 AnimationOffset
    {
        get => Offset;
        set
        {
            Offset = value;
            if (value == Vector2.Zero)
            {
                _frameLoops.Remove(Animation);
                return;
            }
            
            if (_offsets.TryAdd(Animation, value)) return;
            _offsets[Animation] = value;
        }
    }

    private Dictionary<StringName, int> _frameLoops;
    private Dictionary<StringName, Vector2> _offsets;
    
    public StringName NextAnimation { get; set; }
    
    private int _frameLoop;
    
    public override void _Ready()
    {
        _frameLoops = [];
        _offsets = [];
        
        AnimationChanged += UpdateValues;
        AnimationFinished += SetNextAnimation;
        AnimationLooped += LoopFrame;
    }

    private void LoopFrame()
    {
        if (_frameLoop == 0) return;
        Frame = _frameLoop;
    }

    private void UpdateValues()
    {
        _frameLoop = _frameLoops.GetValueOrDefault(Animation);
        Offset = _offsets.GetValueOrDefault(Animation);
    }
    
    private void SetNextAnimation()
    {
        if (NextAnimation == null) return;
        Animation = NextAnimation;
    }
}
