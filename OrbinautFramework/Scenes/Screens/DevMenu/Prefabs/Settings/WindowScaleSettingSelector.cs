using System;
using Godot;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class WindowScaleSettingSelector : SettingSelectorLogic
{
    private const int Limit = 6;
    
    public override string GetText() => Settings.WindowScale.ToString();
    public override void OnLeftPressed() => SetNewWindowScale(-1);
    public override void OnRightPressed() => SetNewWindowScale(1);
    
    private static void SetNewWindowScale(int offset)
    {
        int scale = Settings.WindowScale + offset;
        
        Settings.WindowScale = (byte)(scale switch
        {
            <= 0 => Limit,
            > Limit => 1,
            _ => scale
        });
    }
}
