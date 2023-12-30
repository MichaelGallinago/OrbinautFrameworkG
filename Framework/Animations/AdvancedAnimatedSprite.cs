using System;
using System.Linq;
using Godot.Collections;
using Godot;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

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

            if (CollectionExtensions.TryAdd(_frameLoops, Animation, value)) return;
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
                _offsets.Remove(Animation);
                return;
            }

            if (CollectionExtensions.TryAdd(_offsets, Animation, value)) return;
            _offsets[Animation] = value;
        }
    }

    [ExportGroup("Dictionaries")]
    [Export] private Dictionary<StringName, int> _frameLoops = [];
    [Export] private Dictionary<StringName, Vector2> _offsets = [];

    public StringName NextAnimation { get; set; }
    
    private int _frameLoop;
    private bool _isRuntime;
    
    public override void _Ready()
    {
        _isRuntime = !Engine.IsEditorHint();
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
        _frameLoop = CollectionExtensions.GetValueOrDefault(_frameLoops, Animation);
        Offset = CollectionExtensions.GetValueOrDefault(_offsets, Animation);
        
        if (_isRuntime) return;
        System.Collections.Generic.List<StringName> offsetsKeys = _offsets.Keys.ToList();
        System.Collections.Generic.List<StringName> frameLoopsKeys = _frameLoops.Keys.ToList();
            
        foreach (StringName animation in offsetsKeys)
        {
            if (SpriteFrames.HasAnimation(animation)) continue;
            _offsets.Remove(animation);
        }
            
        foreach (StringName animation in frameLoopsKeys)
        {
            if (SpriteFrames.HasAnimation(animation)) continue;
            _frameLoops.Remove(animation);
        }
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
}
