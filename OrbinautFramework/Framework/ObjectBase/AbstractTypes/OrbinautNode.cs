using Godot;

namespace OrbinautFramework3.Framework.ObjectBase.AbstractTypes;

public abstract partial class OrbinautNode : SolidNode
{
    [Export] public HitBox HitBox { get; private set; }
}
