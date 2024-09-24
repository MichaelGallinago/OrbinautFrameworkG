using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Characters.Tails;

public partial class TailsSpriteLogic(PlayerData player, ISpriteNode spriteNode, CarryData carryData) : 
    SpriteLogic(player, spriteNode)
{
    private readonly PlayerData _player = player;

    protected override void Animate()
    {
        SetType(Data.Type, Data.Speed);
		
        if (Data.Type != Animations.FlyCarry) return;
        Node.Frame = _player.Movement.Velocity.Y <= 0f ? 1 : 0;
    }
    
    protected override void UpdateType() => Data.Type = Data.Animation switch
    {
        Animations.Fly => carryData.Target == null ? Animations.Fly : Animations.FlyCarry,
        Animations.FlyTired => carryData.Target == null ? Animations.FlyTired : Animations.FlyCarryTired,
        Animations.Move => GetMoveAnimation(true, 6f),
        _ => Data.Animation
    };
    
    protected override void UpdateSpeed() => Data.Speed = Data.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Swim => _player.Movement.Velocity.Y <= 0f ? 1f : 0.5f,
        _ => 1f
    };
}
