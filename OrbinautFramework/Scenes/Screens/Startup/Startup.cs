using Godot;

namespace OrbinautFramework3.Scenes.Screens.Startup;

public partial class Startup : Node2D
{
    [Export] private PackedScene _nextScene;
    
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);
    }

    public override void _Process(double delta)
    {
        SceneTree sceneTree = GetTree();

        if (_nextScene == null)
        {
            sceneTree.Quit();
            return;
        }

        sceneTree.ChangeSceneToPacked(_nextScene);
    }
}
