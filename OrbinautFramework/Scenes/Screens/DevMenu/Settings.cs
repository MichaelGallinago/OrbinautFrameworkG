using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class Settings : Menu
{
	public const string ConfigPath = "res://options.cfg";
	private const string Section = "settings";
	private ConfigFile _config = new();

	public override void _Ready()
	{
		base._Ready();
	}

	protected override void Select()
	{
		base.Select();
		Error error = _config.Load(ConfigPath);
		if (error != Error.Ok)
		{
			_config.GetValue(Section, "joypad_rumble", false);
			_config.SetValue(Section, "best_score", 50);
			_config.SetValue(Section, "player_name", 50);
			_config.SetValue(Section, "window_scale", 1);
			_config.SetValue(Section, "frequency", 60);
		}
		
		_config.Save(ConfigPath);
	}

	private void OnVSyncSwitched()
	{
		
	}
}
