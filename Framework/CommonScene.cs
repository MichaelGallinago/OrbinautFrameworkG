using System;
using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonScene : Node2D
{
    private const byte BaseFramerate = 60;

    public bool IsStage { get; protected set; }

    public event Action<double> PreUpdate;
    public event Action<double> EarlyUpdate;
    public event Action<double> Update;
    public event Action<double> LateUpdate;
    private event Action<double> PlayerUpdate;

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
        PreUpdate?.Invoke(processSpeed);
        EarlyUpdate?.Invoke(processSpeed);
        PlayerUpdate?.Invoke(processSpeed);
        Update?.Invoke(processSpeed);
        LateUpdate?.Invoke(processSpeed);
        Animator.Process(processSpeed);
    }

    public void AddPlayerStep(Objects.Player.Player player)
    {
        PlayerUpdate += player.PlayerStep;
    }

    public void RemovePlayerStep(Objects.Player.Player player)
    {
        PlayerUpdate -= player.PlayerStep;
    }
}