using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.CommonObject;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Spawnable.Barrier;

public partial class Barrier(BaseObject target) : AnimatedSprite
{
    public enum Types : byte
    {
        None, Normal, Thunder, Flame, Water
    }
    
    public enum States : byte
    { 
        None, Active, Disabled, DoubleSpin
    }
    
    public float Angle { get; set; }
    public States State { get; set; }
    public BaseObject Target { get; set; } = target;
    public float AnimationTimer { get; set; } = -1f;
    
    private Types _type;
    
    public Types Type
    { 
        get => _type;
        set
        {
            if (_type == value) return;
            if (value == Types.None)
            {
                Target.RemoveChild(this);
            }
            else if (_type == Types.None)
            {
                Target.AddChild(this);
            }
            _type = value;
        } 
    }
}
