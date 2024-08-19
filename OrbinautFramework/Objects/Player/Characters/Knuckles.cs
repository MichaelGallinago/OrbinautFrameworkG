using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Knuckles : PlayerNode
{
    public Knuckles()
    {
        ClimbAnimationFrameNumber = SpriteNode.GetAnimationFrameCount(Animations.ClimbWall, Types.Knuckles);
    }
}