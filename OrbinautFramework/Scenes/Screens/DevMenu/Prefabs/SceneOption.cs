using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class SceneOption : Option
{
    [Export] private PackedScene _scene;
    
    [Signal] public delegate void SelectedSceneEventHandler(PackedScene scene);
    
    public override void _Ready() => PressedSelect += () => EmitSignal(SignalName.SelectedScene, _scene);
}
