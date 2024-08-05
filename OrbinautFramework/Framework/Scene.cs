using System;
using Godot;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public abstract partial class Scene : Node2D
{
    public enum States : byte
    {
        Normal, StopObjects, Paused
    }
    
    public static Scene Instance { get; private set; }
    
    [Export] public CollisionTileMap CollisionTileMapMain { get; private set; }
    [Export] public CollisionTileMap CollisionTileMapSecondary { get; private set; }
    [Export] public Views Views { get; private set; }
    [Export] public PrefabStorage PrefabStorage { get; init; }
    
    public PlayerList Players { get; } = new();
    public int PlayerCount { get; set; }
    
    public SceneTree Tree { get; private set; }
    public World2D World2D { get; private set; }
    public bool IsStage { get; protected set; }
    public ObjectCuller Culler { get; } = new();
    public float ProcessSpeed { get; private set; }
    public float RingSpillTimer { get; set; }
    public bool AllowPause { get; set; }
    public States State { get; set; } = States.Normal;
    public float Time { get; set; }
    
    private SceneContinuousUpdate _sceneContinuousUpdate = new();
    private SceneLateUpdate _lateUpdate = new();
    private Debug _debug = new();
    
    protected Scene()
    {
        PrefabStorage = _prefabStorage.Instantiate();
        ProcessPriority = int.MinValue;
    }

    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_lateUpdate);
        AddChild(_debug);

        Tree = GetTree();

        AttachCamerasToPlayer();
    }
    
    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (Instance != this) return;
        Instance = null;
    }

    public override void _Process(double deltaTime)
    {
        ProcessSpeed = Engine.MaxFps > 60 || Engine.MaxFps == 0 ? 
            Math.Min(1f, (float)(deltaTime * Constants.BaseFramerate)) : 1f;
        
        if (State != States.Paused)
        {
            Time += ProcessSpeed;
        }
        
        Culler.EarlyCull();
        
        foreach (OrbinautData objects in Culler.ActiveObjects)
        {
            objects.PreviousPosition = objects.Position;
        }
        
        foreach (Player player in Players.Values)
        {
            player.TouchObjects.Clear();
            player.PushObjects.Clear();
        }
    }
    
    public bool IsTimePeriodLooped(float period) => Time % period - ProcessSpeed < 0f;
    public bool IsTimePeriodLooped(float period, float offset) => (Time + offset) % period - ProcessSpeed < 0f;

    private void AttachCamerasToPlayer()
    {
        ReadOnlySpan<ICamera> cameras = Views.Cameras;
        int count = Math.Min(cameras.Length, Players.Count);
        for (var i = 0; i < count; i++)
        {
            cameras[i].Target = Players.Values[i];
        }
    }
}
