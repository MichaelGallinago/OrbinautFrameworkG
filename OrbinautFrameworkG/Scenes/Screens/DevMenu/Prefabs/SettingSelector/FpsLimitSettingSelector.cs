using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelector;

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

    public override void OnSelectPressed()
    {
        var screenRefreshRate = (ushort)DisplayServer.ScreenGetRefreshRate();

        ushort targetFps = Settings.TargetFps;
        if (targetFps == 0)
        {
            Settings.TargetFps = screenRefreshRate;
        }
        else if (targetFps == screenRefreshRate)
        {
            Settings.TargetFps = (ushort)(screenRefreshRate * 2);
        }
        else
        {
            Settings.TargetFps = 0;
        }
    }
}
