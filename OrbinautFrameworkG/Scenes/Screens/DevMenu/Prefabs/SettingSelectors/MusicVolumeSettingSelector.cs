using Godot;
using OrbinautFrameworkG.Audio.Player;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelectors;

[GlobalClass]
public partial class MusicVolumeSettingSelector : SettingSelectorLogic
{
    public override string GetText() => PercentVolume.ToString();
    public override void OnLeftPressed() => SetNewVolume(-5);
    public override void OnRightPressed() => SetNewVolume(5);
    
    private static void SetNewVolume(int offset)
    {
        int newVolume = PercentVolume + offset;
        AudioPlayer.Music.MaxVolume = newVolume switch
        {
            < 0 => 1f,
            > 100 => 0f,
            _ => newVolume / 100f
        };
        
        AudioPlayer.Music.PlayJingle(MusicStorage.ActClear);
    }

    private static int PercentVolume => (int)(AudioPlayer.Music.MaxVolume * 100f);
}
