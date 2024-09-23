using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class StartGame : Menu
{
	[Signal] public delegate void SelectedSaveEventHandler(PackedScene scene, byte slot);
	
	public override void OnExit() => EmitSignal(SignalName.SelectedSave, (PackedScene)null, 0);
}
