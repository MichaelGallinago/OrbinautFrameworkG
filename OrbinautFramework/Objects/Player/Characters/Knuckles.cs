namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Knuckles : Player
{
    public Knuckles()
    {
        ClimbAnimationFrameNumber = Sprite.GetAnimationFrameCount(Animations.ClimbWall, Types.Knuckles);
    }
}