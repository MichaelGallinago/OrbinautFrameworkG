using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu;

public partial class SaveSelectMenu : Menu
{
	[Signal] public delegate void SelectedSaveEventHandler(PackedScene scene, byte slot);

	public override void OnExit() => EmitSignal(SignalName.SelectedSave, (PackedScene)null, 0);
}
