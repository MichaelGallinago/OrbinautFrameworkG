using Godot;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public abstract partial class InteractiveNode : CullableNode, IInteractive
{
    public bool IsInteract { get; set; }
    [Export] public HitBox HitBox { get; private set; }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Scene.Instance.FrameStartProcess.Subscribe(this);
    }
    
    public override void _ExitTree()
    {
        base._EnterTree();
        Scene.Instance.FrameStartProcess.Subscribe(this);
    }
}
