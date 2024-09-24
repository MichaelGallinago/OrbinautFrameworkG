using OrbinautFrameworkG.Framework.Animations;

namespace OrbinautFrameworkG.Objects.Spawnable.Shield;

public partial class ShieldContainer : AdvancedAnimatedSprite
{
    public enum Types : byte
    {
        None, Normal, Lightning, Fire, Bubble
    }
    
    public enum States : byte
    {
        None, Active, Disabled, DoubleSpin
    }
    
    public enum AnimationTypes : byte
    {
        None, Normal, Lightning, Fire, FireDash, Bubble, BubbleBounce
    }
    
    public float Angle { get; set; }
    public States State { get; set; }
    public AnimationTypes AnimationType { get; set; }

    public Types Type { get; set; } = Types.None;
}
