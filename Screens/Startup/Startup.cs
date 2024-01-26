using Godot;

namespace OrbinautFramework3.Screens.Startup;

public partial class Startup : Framework.CommonScene
{
    [Export] private ResourcePreloader _preloader;

    private int _time;
    
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);
    }

    public override void _Process(double deltaTime)
    {
        _time++;

        if (_time < 100)
        {
            return;
        }
        
        SceneTree sceneTree = GetTree();
        
        if (_preloader == null)
        {
            sceneTree.Quit();
        }
        else
        {
            Resource scene = _preloader.GetResource("test_stage_zone_0");
            sceneTree.CallDeferred("change_scene_to_packed", scene);
        }
    }
}