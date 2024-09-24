using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu;

public partial class SceneSelect : Menu
{
	[Signal] public delegate void SelectedSceneEventHandler(PackedScene scene);
	
	public override void OnExit() => EmitSignal(SignalName.SelectedScene, (PackedScene)null);
}
