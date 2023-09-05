using Godot;
using System;

public abstract partial class CommonScene : Node2D
{
    private const byte BaseFramerate = 60;
    
    public event Action<double> BeginStep;
    public event Action<double> Step;
    public event Action<double> EndStep;
    private event Action<double> PlayerStep;

    public override void _Ready()
    {
        Animator.Reset();
    }

    public override void _EnterTree()
    {
        FrameworkData.CurrentScene = this;
    }
    
    public override void _ExitTree()
    {
        FrameworkData.CurrentScene = null;
    }

    public override void _Process(double processSpeed)
    {
        processSpeed *= BaseFramerate;
        FrameworkData.ProcessSpeed = processSpeed;
        InputUtilities.Process();
        PlayerStep?.Invoke(processSpeed);
        BeginStep?.Invoke(processSpeed);
        Step?.Invoke(processSpeed);
        EndStep?.Invoke(processSpeed);
        Animator.Process(processSpeed);
    }

    public void AddPlayerStep(Player player)
    {
        PlayerStep += player.PlayerStep;
    }

    public void RemovePlayerStep(Player player)
    {
        PlayerStep -= player.PlayerStep;
    }
}
