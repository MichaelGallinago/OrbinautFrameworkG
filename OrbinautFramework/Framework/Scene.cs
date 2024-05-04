using System;
using Godot;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;

namespace OrbinautFramework3.Framework;

public abstract partial class Scene : Node2D
{
    public static Scene Local { get; private set; }
    
    [Export] public CollisionTileMap CollisionTileMapMain { get; }
    [Export] public CollisionTileMap CollisionTileMapSecondary { get; }
    [Export] public Views Views { get; }
    
    public SceneTree Tree { get; private set; }
    public bool IsStage { get; protected set; }
    public ObjectCuller Culler { get; } = new();
    public float ProcessSpeed { get; private set; }
    public bool AllowPause { get; set; }
    public bool IsPaused { get; set; }
    public bool UpdateObjects { get; set; } = true;
    public float Time { get; set; }
    
    private SceneContinuousUpdate _sceneContinuousUpdate = new();
    private SceneLateUpdate _lateUpdate = new();
    private Debug _debug = new();
    
    protected Scene() => ProcessPriority = int.MinValue;
    
    public override void _Ready()
    {
        AddChild(_sceneContinuousUpdate);
        AddChild(_lateUpdate);
        AddChild(_debug);

        Tree = GetTree();

        AttachCamerasToPlayer();
    }

    public override void _EnterTree() => Local = this;
    public override void _ExitTree() => Local = null;

    public override void _Process(double deltaTime)
    {
        ProcessSpeed = Math.Min(1.0f, (float)(deltaTime * Constants.BaseFramerate));
        
        if (!IsPaused)
        {
            Time += ProcessSpeed;
        }
        
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
    
    public bool IsTimePeriodLooped(float period) => Time % period - ProcessSpeed < 0f;
    public bool IsTimePeriodLooped(float period, float offset) => (Time + offset) % period - ProcessSpeed < 0f;

    private void AttachCamerasToPlayer()
    {
        ReadOnlySpan<ICamera> cameras = Views.Cameras;
        int count = Math.Min(cameras.Length, PlayerData.Players.Count);
        for (var i = 0; i < count; i++)
        {
            cameras[i].Target = PlayerData.Players[i];
        }
    }
}
