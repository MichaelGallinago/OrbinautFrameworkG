using Godot;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Objects.Spawnable.Shield;

public partial class ShieldContainer(Node2D target) : AdvancedAnimatedSprite
{
    public enum Types : byte
    {
        None, Normal, Lightning, Fire, Bubble
    }
    
    public enum States : byte
    { 
        None, Active, Disabled, DoubleSpin
    }
    
    public float Angle { get; set; }
    public States State { get; set; }
    public Node2D Target { get; set; } = target;
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
