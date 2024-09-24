using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu;

public partial class SettingsMenu : Menu
{
	public override void OnExit() => ConfigUtilities.Save();

	private static void OnJoypadRumbleChanged(bool isEnabled) => InputUtilities.JoypadRumble = isEnabled;
	private static void OnMusicVolumeChanged(float volume) => AudioPlayer.Music.MaxVolume = volume;
	private static void OnSoundVolumeChanged(float volume) => AudioPlayer.Sound.MaxVolume = volume;
	private static void OnTargetFpsChanged(ushort value) => Settings.TargetFps = value;
	private static void OnWindowScaleChanged(byte scale) => Settings.WindowScale = scale;
	private static void OnVSyncSModeChanged(long mode) => Settings.VSyncMode = (DisplayServer.VSyncMode)mode;
	private static void OnWindowModeChanged(long mode) => Settings.WindowMode = (DisplayServer.WindowMode)mode;
}
