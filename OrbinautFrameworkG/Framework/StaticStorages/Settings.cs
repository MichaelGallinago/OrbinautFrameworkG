using System;
using Godot;

namespace OrbinautFrameworkG.Framework.StaticStorages;

public static class Settings
{
    public static bool IsInitialized { get; set; }
    
    public static byte WindowScale
    {
        get => _windowScale;
        set => DisplayServer.WindowSetSize(ViewSize * (_windowScale = value));
    }
    private static byte _windowScale = 1;

    public static ushort TargetFps
    {
        get => _targetFps;
        set => Engine.MaxFps = _targetFps = value;
    }
    private static ushort _targetFps;
    
    public static DisplayServer.WindowMode WindowMode 
    {
        get => _windowMode;
        set => DisplayServer.WindowSetMode(_windowMode = value);
    }
    private static DisplayServer.WindowMode _windowMode;
    
    public static DisplayServer.VSyncMode VSyncMode
    {
        get => _vSyncMode;
        set => DisplayServer.WindowSetVsyncMode(_vSyncMode = value);
    }
    private static DisplayServer.VSyncMode _vSyncMode;
    
    public static event Action<Vector2I> ViewSizeChanged;
    public static Vector2I ViewSize
    {
        get => _viewSize;
        set
        {
            ViewSizeChanged?.Invoke(value);
            HalfViewSize = value / 2;
            DisplayServer.WindowSetSize((_viewSize = value) * WindowScale);
        }
    }
    public static Vector2I HalfViewSize { get; private set; }
    private static Vector2I _viewSize;
    
    public static bool SkipBranding { get; set; }
    
    // Default settings. May be overwritten by the config file
    static Settings()
    {
        ViewSize = new Vector2I(400, 224);
        WindowScale = 1;
        TargetFps = 165;
        WindowMode = DisplayServer.WindowMode.Windowed;
        VSyncMode = DisplayServer.VSyncMode.Disabled;
    }
}