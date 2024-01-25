using System.Threading;
using Godot;

namespace OrbinautFramework3.Screens.Startup;

public partial class Startup : Framework.CommonScene
{
    [Export] private ResourcePreloader _preloader;
    
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);

        SceneTree sceneTree = GetTree();
        
        Thread.Sleep(1000);
        
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