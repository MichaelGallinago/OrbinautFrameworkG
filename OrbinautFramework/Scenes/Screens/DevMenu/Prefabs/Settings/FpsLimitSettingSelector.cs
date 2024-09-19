using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class FpsLimitSettingSelector : SettingSelectorLogic
{
    public override string GetText() => Settings.TargetFps == 0 ? "Unlimited" : Settings.TargetFps.ToString();
    
    public override void OnRightPressed()
    {
        if (Settings.TargetFps is >= Constants.BaseFrameRate and < ushort.MaxValue)
        {
            Settings.TargetFps++;
            return;
        }
        
        Settings.TargetFps = Constants.BaseFrameRate;
    }
    
    public override void OnLeftPressed()
    {
        if (Settings.TargetFps > Constants.BaseFrameRate)
        {
            Settings.TargetFps--;
            return;
        }
        
        Settings.TargetFps = 0;
    }
}
