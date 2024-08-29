using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct DataUtilities(PlayerData data)
{
    public void ResetData()
    {
        data.Movement.IsSpinning = false;
        data.Movement.IsGrounded = false;
        data.Movement.IsJumping = false;
		
        data.Visual.SetPushBy = null;
        data.Collision.OnObject = null;
        
        data.Collision.Radius = data.Collision.RadiusNormal;
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
        else if (Stage.Local != null && Stage.Local.Music != null)
        {
            AudioPlayer.Music.Play(Stage.Local.Music);
        }
    }
}
