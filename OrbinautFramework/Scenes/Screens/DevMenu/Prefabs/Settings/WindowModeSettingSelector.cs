using System;
using Godot;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class WindowModeSettingSelector : SettingSelectorLogic
{
    private static readonly DisplayServer.WindowMode[] Variants = [
        DisplayServer.WindowMode.Windowed, 
        DisplayServer.WindowMode.Fullscreen,
        DisplayServer.WindowMode.ExclusiveFullscreen
    ];
    
    public override string GetText() => Settings.WindowMode.ToString();
    public override void OnLeftPressed() => SetVariant(-1);
    public override void OnRightPressed() => SetVariant(1);
    
    private static void SetVariant(int offset)
    {
        int index = Array.IndexOf(Variants, Settings.WindowMode);
        
        if (index == -1)
        {
            index = 0;
        }
        
        index = (index + offset) % Variants.Length;
        
        if (index < 0)
        {
            index = (index + Variants.Length) % Variants.Length;
        }
        
        Settings.WindowMode = Variants[index];
    }
}
