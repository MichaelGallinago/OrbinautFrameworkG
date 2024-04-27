using System;
using Godot;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonScene : Node2D
{
    [Export] public CollisionTileMap CollisionTileMap { get; }
    [Export] public Views Views { get; }
    
    public SceneTree Tree { get; private set; }
    public bool IsStage { get; protected set; }
    public ObjectCuller Culler { get; } = new();
    
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
    }

    public override void _EnterTree() => FrameworkData.CurrentScene = this;
    public override void _ExitTree() => FrameworkData.CurrentScene = null;

    public override void _Process(double deltaTime)
    {
        FrameworkData.ProcessSpeed = Math.Min(1.0f, (float)(deltaTime * Constants.BaseFramerate));
        FrameworkData.UpdateEarly(FrameworkData.ProcessSpeed);
        Culler.EarlyCull();
        
        foreach (BaseObject objects in Culler.ActiveObjects)
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
