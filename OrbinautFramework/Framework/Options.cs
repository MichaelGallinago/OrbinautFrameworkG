using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Framework;

public static class Options
{
    private const string ConfigPath = "res://options.cfg";
    private const string Settings = "settings";
    private static readonly ConfigFile Config = new();

    public static void LoadOptions()
    {
        Error error = Config.Load(ConfigPath);
        if (error != Error.Ok)
        {
            Config.Clear(); 
            return;
        }
        
        if (!Config.HasSection(Settings))
        {
            FillDefaultData();
        }
        
        Config.Clear();
    }

    private static void FillDefaultData()
    {
        JoypadRumble = true;
        MusicVolume = 50f;
        SoundVolume = 50f;
        WindowScale = 1;
        Frequency = Constants.BaseFrameRate;
        WindowMode = DisplayServer.WindowMode.Windowed;
        
        Config.SetValue(Settings, "joypad_rumble", true);
        Config.SetValue(Settings, "music_volume", 50);
        Config.SetValue(Settings, "sound_volume", 50);
        Config.SetValue(Settings, "window_scale", 1);
        Config.SetValue(Settings, "frequency", Constants.BaseFrameRate);
        Config.SetValue(Settings, "fullscreen", (long)DisplayServer.WindowMode.Windowed);
        Config.SetValue(Settings, "vsync", (long)DisplayServer.VSyncMode.Disabled);
        
        SetData(true, 50, 50, 1,
            Constants.BaseFrameRate, DisplayServer.WindowMode.Windowed, DisplayServer.VSyncMode.Disabled);
    }

    public static void SetData(
        bool joypadRumble, float musicVolume, float soundVolume, byte windowScale, ushort frequency, 
        DisplayServer.WindowMode windowMode, DisplayServer.VSyncMode vSyncMode)
    {
        InputUtilities.JoypadRumble = joypadRumble;
        AudioPlayer.Music.Volume = musicVolume;
        AudioPlayer.Sound.Volume = soundVolume;
        DisplayServer.WindowSetSize(SharedData.ViewSize * windowScale);
        Engine.MaxFps = frequency;
        DisplayServer.WindowSetMode(windowMode);
        DisplayServer.WindowSetVsyncMode(vSyncMode);
    }

    public static bool JoypadRumble
    {
        get => InputUtilities.JoypadRumble;
        set
        {
            InputUtilities.JoypadRumble = value;
            Config.SetValue(Settings, "joypad_rumble", value);
        }
    }

    public static float MusicVolume
    {
        get => AudioPlayer.Music.Volume;
        set
        {
            AudioPlayer.Music.Volume = value;
            Config.SetValue(Settings, "music_volume", value);   
        }
    }
    
    public static float SoundVolume
    {
        get => AudioPlayer.Sound.Volume;
        set
        {
            AudioPlayer.Sound.Volume = value;
            Config.SetValue(Settings, "sound_volume", value);   
        }
    }
    
    public static byte WindowScale
    {
        get => SharedData.WindowScale;
        set
        {
            SharedData.WindowScale = value;
            DisplayServer.WindowSetSize(SharedData.ViewSize * SharedData.WindowScale);
            Config.SetValue(Settings, "window_scale", value);   
        }
    }

    public static int Frequency
    {
        get => Engine.MaxFps;
        set
        {
            Engine.MaxFps = value;
            Config.SetValue(Settings, "frequency", value);   
        }
    }
    
    public static DisplayServer.WindowMode WindowMode
    {
        get => SharedData.WindowScale;
        set
        {
            SharedData.WindowScale = value;
            DisplayServer.WindowSetSize(SharedData.ViewSize * SharedData.WindowScale);
            Config.SetValue(Settings, "window_scale", value);   
        }
    }
}
