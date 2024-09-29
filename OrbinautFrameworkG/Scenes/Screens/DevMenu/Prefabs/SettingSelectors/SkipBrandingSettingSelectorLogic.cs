using Godot;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelectors;

[GlobalClass]
public partial class SkipBrandingSettingSelectorLogic : SettingSelectorLogic
{
	public override string GetText() => Settings.SkipBranding.ToString();
	public override void OnLeftPressed() => Settings.SkipBranding = !Settings.SkipBranding;
	public override void OnRightPressed() => OnLeftPressed();
}
