using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Knuckles;

public partial class KnucklesNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
        base._EnterTree();
        SpriteNode.SpriteLogic = new KnucklesSpriteLogic(PlayerLogic, SpriteNode);
    }
}
