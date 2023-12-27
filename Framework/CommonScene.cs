using Godot;
using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.Input;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonScene : Node2D
{
    public bool IsStage { get; protected set; }
    public CollisionTileMap CollisionTileMap { get; set; }
    public SceneLateUpdate LateUpdate = new() { ProcessPriority = int.MaxValue };

    public override void _Ready()
    {
        AddChild(LateUpdate);
        Animator.Reset();
    }

    public override void _EnterTree() => FrameworkData.CurrentScene = this;
    public override void _ExitTree() => FrameworkData.CurrentScene = null;

    public override void _Process(double deltaTime)
    {
        FrameworkData.ProcessSpeed = (float)(deltaTime * Constants.BaseFramerate);
        
        InputUtilities.Process();
        FrameworkData.UpdateEarly(FrameworkData.ProcessSpeed);
        
        foreach (BaseObject objects in BaseObject.Objects)
        {
            objects.PreviousPosition = objects.Position;
        }

        foreach (Player player in Player.Players)
        {
            player.TouchObjects.Clear();
        }
    }
}
