using Godot;

namespace OrbinautFramework3.Screens.Startup;

public partial class Startup : Framework.CommonScene
{
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);
    }
}