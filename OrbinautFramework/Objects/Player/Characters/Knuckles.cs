namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Knuckles : Player
{
    public Knuckles()
    {
        ClimbAnimationFrameNumber = _sprite.GetAnimationFrameCount(Animations.ClimbWall, Types.Knuckles);
    }
}