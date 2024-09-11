using OrbinautFramework3.Objects.Player.Characters.Logic.Base;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Knuckles;

public partial class KnucklesNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode.PlayerSprite);
        PlayerLogic.SetDependencies(new BaseDependencyGenerator(PlayerLogic, PlayerLogic.Data));
        base._EnterTree();
        SpriteNode.SpriteLogic = new KnucklesSpriteLogic(PlayerLogic.Data, SpriteNode);
    }
}
