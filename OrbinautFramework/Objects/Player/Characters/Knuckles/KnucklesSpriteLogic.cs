using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Knuckles;

public partial class KnucklesSpriteLogic(PlayerData player, ISpriteNode spriteNode) :
    SpriteLogic(player, spriteNode)
{
    private readonly PlayerData _player = player;

    protected override void Animate()
    {
        if (Data.Animation == Animations.GlideFall)
        {
            int frame = _player.Visual.OverrideFrame ?? Node.Frame; //TODO: check this
            SetType(Data.Type, frame, Data.Speed);
            return;
        }
		
        SetType(Data.Type, Data.Speed);
    }
    
    protected override void UpdateType() => Data.Type = Data.Animation switch
    {
        Animations.Move => GetMoveAnimation(false, 6f),
        _ => Data.Animation
    };
    
    protected override void UpdateSpeed() => Data.Speed = Data.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Spin => GetGroundSpeed(5f),
        _ => 1f
    };
}
