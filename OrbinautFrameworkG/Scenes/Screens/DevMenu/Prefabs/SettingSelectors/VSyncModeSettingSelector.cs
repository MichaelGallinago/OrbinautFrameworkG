using System;
using Godot;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelectors;

[GlobalClass]
public partial class VSyncModeSettingSelector : SettingSelectorLogic
{
    private static readonly DisplayServer.VSyncMode[] Variants = [
        DisplayServer.VSyncMode.Disabled, DisplayServer.VSyncMode.Enabled, 
        DisplayServer.VSyncMode.Adaptive, DisplayServer.VSyncMode.Mailbox
    ];
    
    public override string GetText() => Settings.VSyncMode.ToString();
    public override void OnLeftPressed() => SetVariant(-1);
    public override void OnRightPressed() => SetVariant(1);
    
    private static void SetVariant(int offset)
    {
        int index = Array.IndexOf(Variants, Settings.VSyncMode);
        
        if (index == -1)
        {
            index = 0;
        }
        
        index = (index + offset) % Variants.Length;
        
        if (index < 0)
        {
            index = (index + Variants.Length) % Variants.Length;
        }
        
        Settings.VSyncMode = Variants[index];
    }
}
