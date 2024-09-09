using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Amy;

public partial class AmySpriteLogic(IPlayer player, ISpriteNode spriteNode) : SpriteLogic(player, spriteNode)
{
    protected override void Animate()
    {
        if (Data.Animation == Animations.HammerSpin)
        {
            SetType(Data.Type, Node.Frame, Data.Speed);
            return;
        }

        SetType(Data.Type, Data.Speed);
    }

    protected override void UpdateType() => Data.Type = Data.Animation switch
    {
        Animations.Move => GetMoveAnimation(true, 6f),
        _ => Data.Animation
    };
    
    protected override void UpdateSpeed() => Data.Speed = Data.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Spin => GetGroundSpeed(5f),
        Animations.HammerSpin => GetGroundSpeed(5f),
        _ => 1f
    };
}
