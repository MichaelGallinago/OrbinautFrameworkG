using Godot;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

[Tool, GlobalClass]
public partial class TailsSpriteLogic : SpriteLogic
{
    protected override void Animate()
    {
        SetType(Data.Type, Data.Speed);
		
        if (Data.Type != Animations.FlyCarry) return;
        Node.Frame = Player.Velocity.Y < 0f ? 1 : 0;
    }
    
    protected override void UpdateType() => Data.Type = Player.Animation switch
    {
        Animations.Fly => Player.Data.Carry.Target == null ? Animations.Fly : Animations.FlyCarry,
        Animations.FlyTired => Player.Data.Carry.Target == null ? Animations.FlyTired : Animations.FlyCarryTired,
        Animations.Move => GetMoveAnimation(true, 6f),
        _ => Player.Animation
    };
    
    protected override void UpdateSpeed() => Data.Speed = Player.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Swim => Player.Velocity.Y < 0f ? 1f : 0.5f,
        _ => 1f
    };
}
