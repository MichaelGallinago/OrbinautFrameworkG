namespace OrbinautFramework3.Objects.Player.Sprite.Characters;

public partial class KnucklesSpriteLogic : SpriteLogic
{
    protected override void Animate()
    {
        if (Player.Animation == Animations.GlideFall)
        {
            int frame = Player.Data.Visual.OverrideFrame ?? Data.Node.Frame; //TODO: check this
            SetType(Data.Type, frame, Data.Speed);
            return;
        }
		
        SetType(Data.Type, Data.Speed);
    }
    
    protected override void UpdateType() => Data.Type = Player.Animation switch
    {
        Animations.Move => GetMoveAnimation(false, 6f),
        _ => Player.Animation
    };
    
    protected override void UpdateSpeed() => Data.Speed = Player.Animation switch
    {
        Animations.Move => GetGroundSpeed(9f),
        Animations.Push => GetGroundSpeed(9f),
        Animations.Spin => GetGroundSpeed(5f),
        _ => 1f
    };
}
