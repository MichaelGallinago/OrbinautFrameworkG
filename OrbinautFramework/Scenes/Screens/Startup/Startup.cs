using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Scenes.Screens.Startup;

public partial class Startup : Scene
{
    [Export] private PackedScene _nextScene;
    
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);

        ChangeScene();
    }

    private void ChangeScene()
    {
        SceneTree sceneTree = GetTree();
        
        if (_nextScene == null)
        {
            sceneTree.Quit();
            return;
        }
        
        sceneTree.CallDeferred("change_scene_to_packed", _nextScene);
    }
}
