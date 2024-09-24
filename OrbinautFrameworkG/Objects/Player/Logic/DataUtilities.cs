using System;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public class DataUtilities(PlayerData data)
{
    public event Action DataReseted;
    
    public void ResetData() 
    {
        MovementData movement = data.Movement;
        movement.IsSpinning = false;
        movement.IsGrounded = false;
        movement.IsJumping = false;
        movement.IsForcedRoll = false;
		
        CollisionData collision = data.Collision;
        collision.OnObject = null;
        collision.Radius = data.Collision.RadiusNormal;

        VisualData visual = data.Visual;
        visual.SetPushBy = null;
        visual.Angle = 0f;

        DataReseted?.Invoke();
    }
	
    public void ResetGravity()
    {
        data.Movement.Gravity = data.Water.IsUnderwater ? GravityType.Underwater : GravityType.Default;
    }
    
    public void ResetMusic()
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
        else if (Zone.Instance != null && Zone.Instance.Music != null)
        {
            AudioPlayer.Music.Play(Zone.Instance.Music);
        }
    }
}
