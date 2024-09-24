using Godot;
using OrbinautFrameworkG.Framework.InputModule;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelector;

[GlobalClass]
public partial class JoypadRumbleSettingSelector : SettingSelectorLogic
{
    public override string GetText() => InputUtilities.JoypadRumble.ToString();
    public override void OnLeftPressed() => InputUtilities.JoypadRumble = !InputUtilities.JoypadRumble;
    public override void OnRightPressed() => OnLeftPressed();
}
