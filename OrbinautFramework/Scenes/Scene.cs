using System;
using DryIoc;
using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player;
using Container = DryIoc.Container;
using Error = Godot.Error;

namespace OrbinautFramework3.Scenes;

public abstract partial class Scene : Node2D, IScene
{
    public enum States : byte
    {
        Normal, StopObjects, Paused
    }
    
    public static float Speed { get; private set; } = 1f;
    
    [Export] public CollisionTileMap CollisionTileMapMain { get; private set; }
    [Export] public CollisionTileMap CollisionTileMapSecondary { get; private set; }
    [Export] private Views _views;

    public IViews Views => _views;

    public PlayerList Players { get; } = new();
    public int PlayerCount { get; set; }
    
    public World2D World2D { get; private set; }
    public bool IsStage { get; protected set; }
    public float RingSpillTimer { get; set; }
    public bool AllowPause { get; set; }
    public States State { get; set; } = States.Normal;
    public float Time { get; set; }

    [UsedImplicitly] private ObjectCuller _culler;
    
    private SceneContinuousUpdate _sceneContinuousUpdate = new();
    private SceneLateUpdate _lateUpdate = new();
    private Debug _debug = new();
    
    private readonly Container _container;
    private SceneTree _tree;
    
    protected Scene()
    {
        ProcessPriority = int.MinValue;
        _container = new Container(rules => rules.With(propertiesAndFields: PropertiesAndFields.All()));
        _container.Register<IScene, Scene>(Reuse.Singleton);
        SetContainer();
    }

    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_lateUpdate);
        AddChild(_debug);

        _tree = GetTree();

        AttachCamerasToPlayer();
    }

    public override void _Process(double deltaTime)
    {
        Speed = Engine.MaxFps > 60 || Engine.MaxFps == 0 ? 
            Math.Min(1f, (float)(deltaTime * Constants.BaseFramerate)) : 1f;
        
        if (State != States.Paused)
        {
            Time += Speed;
        }
        
        _culler.EarlyCull();
        
        foreach (BaseObject objects in _culler.ActiveObjects)
        {
            objects.PreviousPosition = objects.Position;
        }
        
        foreach (Player player in Players.Values)
        {
            player.TouchObjects.Clear();
            player.PushObjects.Clear();
        }
    }

    public void Reload()
    {
        Error error = _tree.ReloadCurrentScene();
        switch (error)
        {
            case Error.Ok: return;
            default: GD.PrintErr("Scene reload error: ", error); break;
        }
    }

    public void Change(string path) => _tree.ChangeSceneToFile(path);
    public bool IsTimePeriodLooped(float period) => Time % period - Speed < 0f;
    public bool IsTimePeriodLooped(float period, float offset) => (Time + offset) % period - Speed < 0f;
    
    private void SetContainer() => StaticContainer.Set(_container);

    private void AttachCamerasToPlayer()
    {
        ReadOnlySpan<ICamera> cameras = _views.Cameras;
        int count = Math.Min(cameras.Length, Players.Count);
        for (var i = 0; i < count; i++)
        {
            cameras[i].Target = Players.Values[i];
        }
    }
}
