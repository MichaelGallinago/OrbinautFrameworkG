using System.Collections.Generic;
using Godot;

public abstract partial class CommonObject : Node2D
{
    public static List<CommonObject> Objects { get; }
    
    public ObjectRespawnData RespawnData { get; set; }
    public InteractData InteractData { get; set; }
    public SolidData SolidData { get; set; }

    static CommonObject()
    {
        Objects = new List<CommonObject>();
    }

    protected CommonObject()
    {
        //TODO: depth
        RespawnData = new ObjectRespawnData(Position, Scale, Visible, 0);
        InteractData = new InteractData();
        SolidData = new SolidData();
    }

    public override void _EnterTree()
    {
        Objects.Add(this);
        
        FrameworkData.CurrentScene.BeginStep += BeginStep;
        FrameworkData.CurrentScene.Step += Step;
        FrameworkData.CurrentScene.EndStep += EndStep;
    }

    public override void _ExitTree()
    {
        Objects.Remove(this);
        
        FrameworkData.CurrentScene.BeginStep -= BeginStep;
        FrameworkData.CurrentScene.Step -= Step;
        FrameworkData.CurrentScene.EndStep -= EndStep;
    }

    protected virtual void BeginStep(double processSpeed) {}
    protected virtual void Step(double processSpeed) {}
    protected virtual void EndStep(double processSpeed) {}

    protected void SetBehaviour(ObjectRespawnData.BehaviourType behaviour)
    {
        if (RespawnData.Behaviour == ObjectRespawnData.BehaviourType.Delete) return;
        RespawnData.Behaviour = behaviour;
    }
}
