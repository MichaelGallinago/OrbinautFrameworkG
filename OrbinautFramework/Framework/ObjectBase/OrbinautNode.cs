using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class OrbinautNode : CullableNode
{
    [Export] public HitBox HitBox { get; init; }
    [Export] public SolidBox SolidBox { get; init; }
    
    public Vector2 PreviousPosition { get; set; }
}
