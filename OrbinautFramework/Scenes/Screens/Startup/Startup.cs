using Godot;
using Timer = OrbinautFramework3.Framework.Timer;

namespace OrbinautFramework3.Scenes.Screens.Startup;

public partial class Startup : Scene
{
    [Export] private PackedScene _nextScene;

    private Timer _timer;
    
    public Startup() => _timer = new Timer(100f, ChangeScene);
    
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);
    }

    public override void _Process(double deltaTime)
    {
        _timer.Update((float)deltaTime);
    }
    
    private void ChangeScene() => GetTree().ChangeSceneToPacked(_nextScene);
}
