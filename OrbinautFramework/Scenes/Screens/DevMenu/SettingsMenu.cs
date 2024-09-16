using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class SettingsMenu : Menu
{
	private static void OnExit() => ConfigUtilities.Save();

	private void OnTargetFpsChanged(ushort value) => Settings.TargetFps = value;
	private void OnWindowScaleChanged(byte scale) => Settings.WindowScale = scale;
	private void OnVSyncSwitched(long mode) => Settings.VSyncMode = (DisplayServer.VSyncMode)mode;
	private void OnWindowModeSwitched(long mode) => Settings.WindowMode = (DisplayServer.WindowMode)mode;
}
