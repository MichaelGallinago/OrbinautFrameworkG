using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Sonic;

public partial class SonicNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
        base._EnterTree();
        SpriteNode.SpriteLogic = new SonicSpriteLogic(PlayerLogic, SpriteNode);
    }
}
