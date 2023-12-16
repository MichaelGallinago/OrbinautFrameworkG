using OrbinautFramework3.Framework.Animations;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Spawnable.Barrier;

public partial class Barrier(CommonObject target) : AnimatedSprite
{
    public enum Types : byte
    {
        None, Normal, Thunder, Flame, Water
    }
    
    public float Angle { get; set; }
    public CommonObject Target { get; set; } = target;
    
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
