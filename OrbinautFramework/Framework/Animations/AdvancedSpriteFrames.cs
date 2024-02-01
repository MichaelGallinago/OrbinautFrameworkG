using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.Animations;

[Tool, GlobalClass]
public partial class AdvancedSpriteFrames : SpriteFrames
{
    [Export] private Godot.Collections.Dictionary<StringName, int> _frameLoops = [];
    [Export] private Godot.Collections.Dictionary<StringName, Vector2> _offsets = [];

    public void Refresh()
    {
        foreach (StringName animation in _frameLoops.Keys)
        {
            if (HasAnimation(animation)) continue;
            _frameLoops.Remove(animation);
        }
        
        foreach (StringName animation in _offsets.Keys)
        {
            if (HasAnimation(animation)) continue;
            _offsets.Remove(animation);
        }
        
        ResourceSaver.Save(this, ResourcePath);
    }

    public void RemoveAnimationOffset(StringName animation)
    {
        _offsets.Remove(animation);
        ResourceSaver.Save(this, ResourcePath);
    }

    public Vector2 GetAnimationOffset(StringName animation) => _offsets.GetValueOrDefault(animation);
    public void SetAnimationOffset(StringName animation, Vector2 offset)
    {
        if (!_offsets.TryAdd(animation, offset))
        {
            _offsets[animation] = offset;
        }
        
        ResourceSaver.Save(this, ResourcePath);
    }

    public void RemoveAnimationFrameLoop(StringName animation)
    {
        _frameLoops.Remove(animation);
        ResourceSaver.Save(this, ResourcePath);
    }

    public int GetAnimationFrameLoop(StringName animation) => _frameLoops.GetValueOrDefault(animation);
    public void SetAnimationFrameLoop(StringName animation, int frame)
    {
        if (!_frameLoops.TryAdd(animation, frame))
        {
            _frameLoops[animation] = frame;
        }
        
        ResourceSaver.Save(this, ResourcePath);
    }
}
