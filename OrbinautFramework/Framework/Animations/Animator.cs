using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.Animations;

public static class Animator
{
    public static Dictionary<AnimatedSprite, float> AutoAnimatedSprites { get; }
    public static Dictionary<AnimatedSprite, float> SyncSprites { get; }
    public static bool IsUpdating { get; private set; }
    public static float SyncTimer { get; private set; }

    static Animator()
    {
        IsUpdating = true;
        AutoAnimatedSprites = new Dictionary<AnimatedSprite, float>();
        AutoAnimatedSprites = new Dictionary<AnimatedSprite, float>();
    }

    public static void Reset()
    {
        IsUpdating = true;
        SyncTimer = 0f;
    }

    public static void Update(float processSpeed)
    {
        //TODO: check this
        // Process sprite animation
        bool targetFlag = !FrameworkData.IsPaused && FrameworkData.UpdateAnimations;
		
        if (IsUpdating != targetFlag)
        {
            foreach (KeyValuePair<AnimatedSprite, float> data in AutoAnimatedSprites)
            {
                data.Key.SpeedScale = targetFlag ? data.Value : 0f;
            }

            IsUpdating = targetFlag;
        }
		
        if (!targetFlag) return;

        SyncTimer += processSpeed;

        foreach (AnimatedSprite sprite in AnimatedSprite.Sprites)
        {
            int duration = sprite.Duration[sprite.Index];
            bool isCustomOrder = sprite.Order.Length > 0;

            sprite.Frame = isCustomOrder ? sprite.Order[sprite.Index] : sprite.Index;
            
            if (!sprite.Sync && sprite.Timer <= 0d)
            {
                sprite.Timer = duration;
            }

            if (duration <= 0) continue;
            int loopIndex = isCustomOrder ? sprite.Order.Length : GetSpriteFrameCount(sprite);
            if (sprite.Sync)
            {
                sprite.Index = (int)(SyncTimer / Mathf.Abs(duration)) % loopIndex;
            }
            else if (sprite.Timer > 0d)
            {
                sprite.Timer -= processSpeed;
            }
            else if (++sprite.Index >= loopIndex)
            {
                sprite.Index = sprite.LoopFrame;
            }
        }
    }

    private static int GetSpriteFrameCount(AnimatedSprite2D sprite)
    {
        return sprite.SpriteFrames.GetFrameCount(sprite.Animation);
    }
}