using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace OrbinautFrameworkG.Framework.Animations;

[Tool, GlobalClass]
public partial class AdvancedSpriteFrames : SpriteFrames
{
    [Export] private Godot.Collections.Dictionary<StringName, int> _frameLoops = [];
    [Export] private Godot.Collections.Dictionary<StringName, Vector2> _offsets = [];
    [Export] public Vector2I CullSize { get; private set; } = Vector2I.Zero;

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
    
    public void Refresh()
    {
        RefreshFrameLoops();
        RefreshOffsets();
        RefreshCullSize();
        
        ResourceSaver.Save(this, ResourcePath);
    }

    private void RefreshFrameLoops()
    {
        foreach (StringName animation in _frameLoops.Keys)
        {
            if (HasAnimation(animation)) continue;
            _frameLoops.Remove(animation);
        }
    }
    
    private void RefreshOffsets()
    {
        foreach (StringName animation in _offsets.Keys)
        {
            if (HasAnimation(animation)) continue;
            _offsets.Remove(animation);
        }
    }

    private void RefreshCullSize()
    {
        int width = CullSize.X;
        int height = CullSize.Y;
           
        Parallel.ForEach(Animations, animation =>
        {
            var animationName = (StringName)animation;
            if (animationName.IsEmpty) return;
            
            for (var i = 0; i < GetFrameCount(animationName); i++)
            {
                var size = (Vector2I)GetFrameTexture(animationName, i).GetSize();
                
                InterlockedWrapper.SetIfGreater(ref width, size.X);
                InterlockedWrapper.SetIfGreater(ref height, size.Y);
            }
        });
        
        CullSize = new Vector2I(width, height);
    }
}
