using Godot;
using System;

public partial class Startup : CommonScene
{
    public override void _Ready()
    {
        PhysicsServer2D.SetActive(false);
        PhysicsServer3D.SetActive(false);
    }
}
