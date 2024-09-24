using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs;

public partial class SceneOption : Option
{
    [Signal] public delegate void SelectedSceneEventHandler(PackedScene scene);
    
    [Export] private PackedScene _scene;
    
    public override void _Ready() => PressedSelect += () => EmitSignal(SignalName.SelectedScene, _scene);
}
