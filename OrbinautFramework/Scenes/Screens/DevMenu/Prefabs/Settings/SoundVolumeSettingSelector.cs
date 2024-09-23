using Godot;
using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class SoundVolumeSettingSelector : SettingSelectorLogic
{
    public override string GetText() => PercentVolume.ToString();
    public override void OnLeftPressed() => SetNewVolume(-5);
    public override void OnRightPressed() => SetNewVolume(5);
    
    private static void SetNewVolume(int offset)
    {
        int newVolume = PercentVolume + offset;
        AudioPlayer.Sound.MaxVolume = newVolume switch
        {
            < 0 => 1f,
            > 100 => 0f,
            _ => newVolume / 100f
        };
        
        AudioPlayer.Sound.Play(SoundStorage.RingLeft);
        AudioPlayer.Sound.Play(SoundStorage.RingRight);
    }

    private static int PercentVolume => (int)(AudioPlayer.Sound.MaxVolume * 100f);
}
