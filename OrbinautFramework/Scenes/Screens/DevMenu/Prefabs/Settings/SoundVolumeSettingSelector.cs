using System;
using Godot;
using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class SoundVolumeSettingSelector : SettingSelectorLogic
{
    public override string GetText() => ((int)AudioPlayer.Sound.MaxVolume).ToString();
    public override void OnLeftPressed() => SetNewVolume(-5f);
    public override void OnRightPressed() => SetNewVolume(5f);
    
    private static void SetNewVolume(float offset)
    {
        AudioPlayer.Sound.MaxVolume = Math.Clamp(AudioPlayer.Sound.MaxVolume + offset, 0f, 100f);
    }
}
