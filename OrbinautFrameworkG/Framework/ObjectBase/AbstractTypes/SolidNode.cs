using Godot;
using OrbinautFrameworkG.Framework.SceneModule;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public abstract partial class SolidNode : CullableNode, IPreviousPosition, ISolid
{
    [Export] public SolidBox SolidBox { get; private set; }
    
    public Vector2 PreviousPosition { get; set; }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        Scene.Instance.FrameEndProcess.Subscribe(this);
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        Scene.Instance.FrameEndProcess.Unsubscribe(this);
    }
}
