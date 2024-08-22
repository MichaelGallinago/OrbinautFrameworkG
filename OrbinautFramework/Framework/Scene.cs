using System;
using Godot;
using OrbinautFramework3.Framework.MultiTypeDelegate;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework;

public partial class Scene : Node2D
{
    public enum States : byte
    {
        Normal, StopObjects, Paused
    }
    
    public static Scene Instance { get; private set; }
    
    [Export] public CollisionTileMap CollisionTileMapMain { get; private set; }
    [Export] public CollisionTileMap CollisionTileMapSecondary { get; private set; }
    [Export] public Views Views { get; private set; }
    [Export] public PlayerPrefabs PlayerPrefabs { get; private set; }
    [Export] public PackedScene[] DebugModePrefabs { get; private set; }
    
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
    
    public IMultiTypeEvent<ITypeDelegate> FrameEndProcess { get; }
    
    private SceneContinuousUpdate _sceneContinuousUpdate = new();
    private SceneFrameEnd _frameEnd = new();
    
#if DEBUG
    private Debug _debug = new();
#endif
    
    protected Scene()
    {
        FrameEndProcess = _frameEnd.Process;
        ProcessPriority = int.MinValue;
    }

    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_frameEnd);
        
        Tree = GetTree();

        AttachCamerasToPlayer();
        
#if DEBUG
        AddChild(_debug);
#endif
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
        ProcessSpeed = Engine.MaxFps is <= 60 and > 0 ? 1f : Math.Min(1f, (float)(deltaTime * Constants.BaseFramerate));
        
        if (State != States.Paused)
        {
            Time += ProcessSpeed;
        }
        
        Culler.EarlyCull();
        
        foreach (IPlayer player in Players.Values)
        {
            player.Data.Collision.TouchObjects.Clear();
            player.Data.Collision.PushObjects.Clear();
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
            cameras[i].Target = Players.Values[i].Data;
        }
    }
}
