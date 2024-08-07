using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Music
{
    public void Reset()
    {
        if (Super.IsSuper)
        {
            AudioPlayer.Music.Play(MusicStorage.Super);
        }
        else if (Item.InvincibilityTimer > 0f)
        {
            AudioPlayer.Music.Play(MusicStorage.Invincibility);
        }
        else if (Item.SpeedTimer > 0f)
        {
            AudioPlayer.Music.Play(MusicStorage.HighSpeed);
        }
        else if (Stage.Local != null && Stage.Local.Music != null)
        {
            AudioPlayer.Music.Play(Stage.Local.Music);
        }
    }
}
