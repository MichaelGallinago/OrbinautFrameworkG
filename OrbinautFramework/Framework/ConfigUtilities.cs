using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Framework;

public static class ConfigUtilities
{
    private const string ConfigPath = "res://options.cfg";
    private const string SectionSettings = "settings";
    private static readonly ConfigFile Config = new();

    public static void Load()
    {
        Error error = Config.Load(ConfigPath);
        if (error != Error.Ok)
        {
            Config.Clear(); 
            return;
        }
        
        if (!Config.HasSection(SectionSettings))
        {
            Save();
        }
        else
        {
            Dto dto = Get();
            SetData(ref dto);
        }
        
        Config.Clear();
    }

    public static void Save()
    {
        var dto = new Dto(
            InputUtilities.JoypadRumble, 
            AudioPlayer.DefaultMusicVolume, 
            AudioPlayer.DefaultSoundVolume,
            Settings.WindowScale,
            Settings.TargetFps,
            Settings.WindowMode,
            Settings.VSyncMode
        );
        
        Set(ref dto);
    }

    private static void SetData(ref Dto dto)
    {
        InputUtilities.JoypadRumble = dto.JoypadRumble;
        AudioPlayer.Music.Volume = dto.MusicVolume;
        AudioPlayer.Sound.Volume = dto.SoundVolume;
        Settings.WindowScale = dto.WindowScale;
        Settings.TargetFps = dto.Frequency;
        Settings.WindowMode = dto.WindowMode;
        Settings.VSyncMode = dto.VSyncMode;
    }

    private static void Set(ref Dto dto)
    {
        Config.SetValue(SectionSettings, "joypad_rumble", dto.JoypadRumble);
        Config.SetValue(SectionSettings, "music_volume", dto.MusicVolume);
        Config.SetValue(SectionSettings, "sound_volume", dto.SoundVolume);
        Config.SetValue(SectionSettings, "window_scale", dto.WindowScale);
        Config.SetValue(SectionSettings, "frequency", dto.Frequency);
        Config.SetValue(SectionSettings, "fullscreen", (long)dto.WindowMode);
        Config.SetValue(SectionSettings, "vsync", (long)dto.VSyncMode);
    }

    private static Dto Get() => new(
        (bool)Config.GetValue(SectionSettings, "joypad_rumble"),
        (float)Config.GetValue(SectionSettings, "music_volume"),
        (float)Config.GetValue(SectionSettings, "sound_volume"),
        (byte)Config.GetValue(SectionSettings, "window_scale"),
        (ushort)Config.GetValue(SectionSettings, "frequency"),
        (DisplayServer.WindowMode)(long)Config.GetValue(SectionSettings, "fullscreen"),
        (DisplayServer.VSyncMode)(long)Config.GetValue(SectionSettings, "vsync")
    );

    private readonly record struct Dto(
        bool JoypadRumble, 
        float MusicVolume, 
        float SoundVolume,
        byte WindowScale,
        ushort Frequency, 
        DisplayServer.WindowMode WindowMode, 
        DisplayServer.VSyncMode VSyncMode
    );
}
