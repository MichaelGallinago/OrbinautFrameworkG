using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;
using OrbinautFrameworkG.Framework.View;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Framework;

public partial class Scene : Node2D
{
    public enum States : byte { Normal, StopObjects, Paused }
    
    public static Scene Instance { get; private set; }
    
    [Export] public Tiles.CollisionTileMap CollisionTileMapMain { get; private set; }
    [Export] public Tiles.CollisionTileMap CollisionTileMapSecondary { get; private set; }
    [Export] public Views Views { get; private set; }
    [Export] public PackedScene[] DebugModePrefabs { get; private set; }
    [Export] public LimitedSize Size { get; private set; } = new();
    
    public PlayerList Players { get; } = new();
    
    public SceneTree Tree { get; private set; }
    public ObjectCuller Culler { get; } = new();
    public float Speed { get; private set; }
    public float RingSpillTimer { get; set; }
    public bool AllowPause { get; set; }
    public States State { get; set; } = States.Normal;
    public float Time { get; private set; }
    
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
        SetInstance();
    }
    
    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_frameEnd);
        
        Tree = GetTree();
        
#if DEBUG
        AddChild(_debug);
#endif
    }
    
    private void SetInstance()
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
        Speed = DeltaTimeUtilities.CalculateSpeed(deltaTime);
        
        if (State != States.Paused)
        {
            Time += Speed;
        }
        
        Culler.EarlyCull();
        
        foreach (IPlayer player in Players.Values)
        {
            player.Data.Collision.TouchObjects.Clear();
            player.Data.Collision.PushObjects.Clear();
        }
    }

    public bool IsTimePeriodLooped(float period) => Time % period - Speed < 0f;
    public bool IsTimePeriodLooped(float period, float offset) => (Time + offset) % period - Speed < 0f;
}
