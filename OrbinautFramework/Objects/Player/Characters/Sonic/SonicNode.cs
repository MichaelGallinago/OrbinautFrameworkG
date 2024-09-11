using OrbinautFramework3.Objects.Player.Characters.Logic.Base;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Sonic;

public partial class SonicNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
        PlayerLogic.SetDependencies(new BaseDependencyGenerator(PlayerLogic, PlayerLogic.Data));
        base._EnterTree();
        SpriteNode.SpriteLogic = new SonicSpriteLogic(PlayerLogic.Data, SpriteNode);
    }
}
