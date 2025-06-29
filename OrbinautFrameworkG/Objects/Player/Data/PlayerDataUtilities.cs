﻿using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Data;

public static class PlayerDataUtilities
{
    public static void ResetGravity(this PlayerData data)
    {
        data.Movement.Gravity = data.Water.IsUnderwater ? GravityType.Underwater : GravityType.Default;
    }
    
    public static void ResetMusic(this PlayerData data)
    {
        if (data.Super.IsSuper)
        {
            AudioPlayer.Music.Play(MusicStorage.Super);
        }
        else if (data.Item.InvincibilityTimer > 0f)
        {
            AudioPlayer.Music.Play(MusicStorage.Invincibility);
        }
        else if (data.Item.SpeedTimer > 0f)
        {
            AudioPlayer.Music.Play(MusicStorage.HighSpeed);
        }
        else if (Zone.Instance != null)
        {
            if (Zone.Instance.Music != null)
            {
                AudioPlayer.Music.Play(Zone.Instance.Music);
                return;
            }
            
            AudioPlayer.Music.StopAll();
        }
    }
}
