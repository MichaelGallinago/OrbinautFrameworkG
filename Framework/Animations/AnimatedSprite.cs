using System;
using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Framework.Animations;

public partial class AnimatedSprite : AnimatedSprite2D
{
    public static List<AnimatedSprite> Sprites { get; }
    
    public bool Sync { get; set; }
    public int LoopFrame { get; set; }
    public double Timer { get; set; }
    public int Index { get; set; }
    public int[] Duration { get; set; }
    public int[] Order { get; set; }
    public AnimationRespawnData RespawnData { get; set; }

    static AnimatedSprite()
    {
        Sprites = [];
    }
    
    public AnimatedSprite()
    {
        SpeedScale = 0f;
        Index = Frame;
        Duration = new[] { 0 };
        Order = Array.Empty<int>();
        RespawnData = new AnimationRespawnData(Frame, Animation, Visible);
    }

    public override void _EnterTree()
    {
        Sprites.Add(this);
        if (GetParent() is CommonObject.CommonObject commonObject)
        {
            commonObject.Sprite ??= this;
            return;
        }
        
        Animator.AutoAnimatedSprites.Add(this, SpeedScale);
    }

    public override void _ExitTree()
    {
        Sprites.Remove(this);
        if (GetParent() is CommonObject.CommonObject commonObject && commonObject.Sprite == this)
        {
            commonObject.Sprite = null;
        }
    }

    public int GetDuration() => Duration[Index];
    public double GetTimer() => Timer == 0 ? Duration[Index] : Timer;

    public void SetAnimation(StringName animationName, int[] duration = null, 
        int startFrame = 0, int loopFrame = 0, int[] order = null)
    {	
        if (!Sync && Animation == animationName) return;
        SetAnimationData(animationName, duration, order);
        Sync = false;
        Index = startFrame;
        LoopFrame = loopFrame;
    }

    public void SetSyncAnimation(StringName animation, int[] duration = null, int[] order = null)
    {
        if (!Sync && Animation == animation) return;
        SetAnimationData(animation, duration, order);
        Sync = true;
    }

    public void UpdateFrame(int frame, int? loopFrame = null, int[] order = null, bool resetTimer = true)
    {
        if (loopFrame != null)
        {
            LoopFrame = (int)loopFrame;
        }

        if (order != null)
        {
            Order = order;
        }

        if (resetTimer)
        {
            Timer = 0d;
        }

        Index = frame;
        Sync = false;
    }
    
    public void UpdateDuration(int[] duration)
    {
        duration ??= new[] { 0 }; // TODO: check if not needed
        
        if (Timer < 0d)
        {
            Timer = duration[0];
        }
	
        Duration = duration;
    }

    public bool CheckInView()
    {
        Vector4I bounds = Camera.MainCamera.Bounds;
        Vector2 size = SpriteFrames.GetFrameTexture(Animation, Frame).GetSize();
        
        return Position.X >= bounds.X - size.X && Position.X <= bounds.Z + size.X &&
               Position.Y >= bounds.Y - size.Y && Position.Y <= bounds.W + size.Y;
    }

    private void SetAnimationData(StringName animation, int[] duration, int[] order)
    {
        Animation = animation;
        SpeedScale = 0f;
        
        Timer = 0d;
        Order = order ?? Array.Empty<int>();
        Duration = duration ?? new[] { 0 };
    }
}