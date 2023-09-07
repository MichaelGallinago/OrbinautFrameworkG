using Godot;
using System;

public abstract partial class CommonScene : Node2D
{
    private const byte BaseFramerate = 60;

    public bool IsStage { get; protected set; }
    
    public event Action<double> EarlyUpdate;
    public event Action<double> Update;
    public event Action<double> LateUpdate;
    private event Action<double> PlayerUpdate;

    protected CommonScene()
    {
        IsStage = false;
    }

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
        EarlyUpdate?.Invoke(processSpeed);
        PlayerUpdate?.Invoke(processSpeed);
        Update?.Invoke(processSpeed);
        LateUpdate?.Invoke(processSpeed);
        Animator.Process(processSpeed);
    }

    public void AddPlayerStep(Player player)
    {
        PlayerUpdate += player.PlayerStep;
    }

    public void RemovePlayerStep(Player player)
    {
        PlayerUpdate -= player.PlayerStep;
    }
}
