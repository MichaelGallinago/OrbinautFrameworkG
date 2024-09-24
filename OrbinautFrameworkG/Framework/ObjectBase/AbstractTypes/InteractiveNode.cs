using Godot;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public partial class InteractiveNode : CullableNode
{
    [Export] public HitBox HitBox { get; private set; }
}
