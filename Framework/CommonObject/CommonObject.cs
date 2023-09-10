using System;
using System.Collections.Generic;
using Godot;

public abstract partial class CommonObject : Node2D
{
    public enum BehaviourType : byte
    {
        Active, Reset, Pause, Delete, Unique
    }
    
    public static List<CommonObject> Objects { get; }

    [Export] public BehaviourType Behaviour;
    
    public ObjectRespawnData RespawnData { get; }
    public InteractData InteractData { get; }
    [Export] public SolidData SolidData;
    public AnimatedSprite Sprite { get; set; }
 
    static CommonObject()
    {
        Objects = new List<CommonObject>();
    }

    protected CommonObject()
    {
        RespawnData = new ObjectRespawnData(Position, Scale, Visible, ZIndex);
        InteractData = new InteractData();
    }

    public override void _EnterTree()
    {
        Objects.Add(this);
        
        FrameworkData.CurrentScene.EarlyUpdate += BeginStep;
        FrameworkData.CurrentScene.Update += Step;
        FrameworkData.CurrentScene.LateUpdate += EndStep;
    }

    public override void _ExitTree()
    {
        Objects.Remove(this);
        
        FrameworkData.CurrentScene.EarlyUpdate -= BeginStep;
        FrameworkData.CurrentScene.Update -= Step;
        FrameworkData.CurrentScene.LateUpdate -= EndStep;
    }

    protected virtual void BeginStep(double processSpeed) {}
    protected virtual void Step(double processSpeed) {}
    protected virtual void EndStep(double processSpeed) {}

    public void SetBehaviour(BehaviourType behaviour)
    {
        if (Behaviour == BehaviourType.Delete) return;
        Behaviour = behaviour;
    }

    public void ResetZIndex()
    {
        ZIndex = RespawnData.ZIndex;
    }
    
    public void SetSolid(Vector2I radius, Vector2I offset = new())
    {
        SolidData.Radius = radius;
        SolidData.Offset = offset;
        SolidData.HeightMap = null;
    }
}
