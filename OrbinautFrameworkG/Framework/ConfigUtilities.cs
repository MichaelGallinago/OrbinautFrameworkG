using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework;

public static class ConfigUtilities
{
    private const string ConfigPath = "user://options.cfg";
    private const string SectionSettings = "settings";
    private static readonly ConfigFile Config = new();

    public static void Load()
    {
        Error error = Config.Load(ConfigPath);
        if (error != Error.Ok)
        {
            if (error == Error.FileNotFound)
            {
                Save();
            }
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
            AudioPlayer.Music.MaxVolume, 
            AudioPlayer.Sound.MaxVolume,
            Settings.WindowScale,
            Settings.TargetFps,
            (long)Settings.WindowMode,
            (long)Settings.VSyncMode,
            Settings.SkipBranding
        );
        
        Set(ref dto);
        Config.Save(ConfigPath);
    }

    private static void SetData(ref Dto dto)
    {
        InputUtilities.JoypadRumble = dto.JoypadRumble;
        AudioPlayer.Music.MaxVolume = dto.MusicVolume;
        AudioPlayer.Sound.MaxVolume = dto.SoundVolume;
        Settings.WindowScale = dto.WindowScale;
        Settings.TargetFps = dto.Frequency;
        Settings.WindowMode = (DisplayServer.WindowMode)dto.WindowMode;
        Settings.VSyncMode = (DisplayServer.VSyncMode)dto.VSyncMode;
        Settings.SkipBranding = dto.SkipBranding;
    }
    
    private static void Set(ref Dto dto)
    {
        Config.SetValue(SectionSettings, "joypad_rumble", dto.JoypadRumble);
        Config.SetValue(SectionSettings, "music_volume", dto.MusicVolume);
        Config.SetValue(SectionSettings, "sound_volume", dto.SoundVolume);
        Config.SetValue(SectionSettings, "window_scale", dto.WindowScale);
        Config.SetValue(SectionSettings, "frequency", dto.Frequency);
        Config.SetValue(SectionSettings, "fullscreen", dto.WindowMode);
        Config.SetValue(SectionSettings, "vsync", dto.VSyncMode);
        Config.SetValue(SectionSettings, "skip_branding", dto.SkipBranding);
    }
    
    private static Dto Get() => new(
        (bool)Config.GetValue(SectionSettings, "joypad_rumble"),
        (float)Config.GetValue(SectionSettings, "music_volume"),
        (float)Config.GetValue(SectionSettings, "sound_volume"),
        (byte)Config.GetValue(SectionSettings, "window_scale"),
        (ushort)Config.GetValue(SectionSettings, "frequency"),
        (long)Config.GetValue(SectionSettings, "fullscreen"),
        (long)Config.GetValue(SectionSettings, "vsync"),
        (bool)Config.GetValue(SectionSettings, "skip_branding")
    );
    
    private readonly record struct Dto(
        bool JoypadRumble, 
        float MusicVolume, 
        float SoundVolume,
        byte WindowScale,
        ushort Frequency, 
        long WindowMode, 
        long VSyncMode,
        bool SkipBranding
    );
}
