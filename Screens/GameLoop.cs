using Godot;
using System;

public partial class GameLoop : Node2D
{
    public event Action<double> BeginStep;
    public event Action<double> Step;
    public event Action<double> EndStep;

    public override void _Process(double delta)
    {
        InputData.Process();
        BeginStep?.Invoke(delta);
        Step?.Invoke(delta);
        EndStep?.Invoke(delta);
    }
}
