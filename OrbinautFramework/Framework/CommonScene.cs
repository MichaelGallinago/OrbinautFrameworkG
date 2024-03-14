using System;
using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonScene : Node2D
{
    [Export] public CollisionTileMap CollisionTileMap { get; set; }
    
    public SceneTree Tree { get; private set; }
    public bool IsStage { get; protected set; }
    
    private SceneContinuousUpdate _sceneContinuousUpdate = new();
    private SceneLateUpdate _lateUpdate = new();
    private Debug _debug = new();
    
    protected CommonScene() => ProcessPriority = int.MinValue;
    
    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_lateUpdate);
        AddChild(_debug);

        Tree = GetTree();
        Animator.Reset();
    }

    public override void _EnterTree() => FrameworkData.CurrentScene = this;
    public override void _ExitTree() => FrameworkData.CurrentScene = null;

    public override void _Process(double deltaTime)
    {
        FrameworkData.ProcessSpeed = Math.Min(1.0f, (float)(deltaTime * Constants.BaseFramerate));
        FrameworkData.UpdateEarly(FrameworkData.ProcessSpeed);
        
        foreach (BaseObject objects in BaseObject.Objects)
        {
            objects.PreviousPosition = objects.Position;
        }

        foreach (Player player in PlayerData.Players)
        {
            player.TouchObjects.Clear();
            player.PushObjects.Clear();
        }
    }
}
