using System;
using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonScene : Node2D
{
    private const byte BaseFramerate = 60;

    public bool IsStage { get; protected set; }
    public CollisionTileMap CollisionTileMap { get; set; }

    public event Action<float> PreUpdate;
    public event Action<float> EarlyUpdate;
    public event Action<float> Update;
    public event Action<float> LateUpdate;
    private event Action<float> PlayerUpdate;

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

    public override void _Process(double deltaTime)
    {
        var processSpeed = (float)(deltaTime * BaseFramerate);
        FrameworkData.ProcessSpeed = processSpeed;
        InputUtilities.Process();
        PreUpdate?.Invoke(processSpeed);
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