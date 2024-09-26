using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public partial class SolidNode : CullableNode, IPreviousPosition, ISolid
{
    [Export] public SolidBox SolidBox { get; private set; }
    
    public Vector2 PreviousPosition { get; set; }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        SceneModule.Scene.Instance.FrameEndProcess.Subscribe(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        SceneModule.Scene.Instance.FrameEndProcess.Unsubscribe(this);
    }

    public bool IsInstanceValid() => IsInstanceValid(this);
}
