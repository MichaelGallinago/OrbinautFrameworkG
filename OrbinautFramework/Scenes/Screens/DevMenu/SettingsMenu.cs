using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Framework.StaticStorages;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class SettingsMenu : Menu
{
	private static void OnExit() => ConfigUtilities.Save();
	
	private static void OnJoypadRumbleChanged(bool isEnabled) => InputUtilities.JoypadRumble = isEnabled;
	private static void OnMusicVolumeChanged(float volume) => AudioPlayer.Music.MaxVolume = volume;
	private static void OnSoundVolumeChanged(float volume) => AudioPlayer.Sound.MaxVolume = volume;
	private static void OnTargetFpsChanged(ushort value) => Settings.TargetFps = value;
	private static void OnWindowScaleChanged(byte scale) => Settings.WindowScale = scale;
	private static void OnVSyncSModeChanged(long mode) => Settings.VSyncMode = (DisplayServer.VSyncMode)mode;
	private static void OnWindowModeChanged(long mode) => Settings.WindowMode = (DisplayServer.WindowMode)mode;
}
