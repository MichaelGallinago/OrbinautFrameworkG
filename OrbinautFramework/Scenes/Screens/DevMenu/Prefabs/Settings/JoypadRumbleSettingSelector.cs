using Godot;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass]
public partial class JoypadRumbleSettingSelector : SettingSelectorLogic
{
    public override string GetText() => InputUtilities.JoypadRumble.ToString();
    public override void OnLeftPressed() => InputUtilities.JoypadRumble = !InputUtilities.JoypadRumble;
    public override void OnRightPressed() => OnLeftPressed();
}
