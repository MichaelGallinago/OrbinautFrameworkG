using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Animator
{
    public static List<AnimatedSprite> Sprites { get; private set; }
    public static double Timer { get; private set; }

    public static void Reset()
    {
        Sprites = new List<AnimatedSprite>();
    }

    public static void Process(double processSpeed)
    {
        //TODO: this
        /*
        if (update_flag != other.update_graphics)
        {
            for (var i = 0; i < array_length(sprite_array); i += 2)
            {
                var _speed = 0;
                if other.update_graphics
                {
                    _speed = 1 / sprite_array[i + 1];
                }
                sprite_set_speed(sprite_array[i], _speed, spritespeed_framespergameframe);
            }
			
            update_flag = other.update_graphics;
        }
		
        if (!other.update_graphics)
        {
            break;
        }
        */
		
        Timer += processSpeed;

        foreach (AnimatedSprite sprite in Sprites)
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
                sprite.Index = (int)Timer / Mathf.Abs(duration) % loopIndex;
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
