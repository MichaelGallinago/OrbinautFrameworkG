using Godot;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public abstract partial class InteractiveSolidNode : SolidNode
{
    [Export] public HitBox HitBox { get; private set; }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Scene.Instance.FrameStartProcess.Subscribe(HitBox);
    }
    
    public override void _ExitTree()
    {
        base._EnterTree();
        Scene.Instance.FrameStartProcess.Subscribe(HitBox);
    }
}
