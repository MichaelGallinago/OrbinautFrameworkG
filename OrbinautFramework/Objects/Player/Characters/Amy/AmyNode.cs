using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Amy;

public partial class AmyNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
        base._EnterTree();
        SpriteNode.SpriteLogic = new AmySpriteLogic(PlayerLogic, SpriteNode);
    }
}
