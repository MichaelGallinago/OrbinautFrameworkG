using Godot;

namespace OrbinautFramework3.Framework.ObjectBase.AbstractTypes;

public partial class InteractiveNode : CullableNode
{
    [Export] public HitBox HitBox { get; init; }
}
