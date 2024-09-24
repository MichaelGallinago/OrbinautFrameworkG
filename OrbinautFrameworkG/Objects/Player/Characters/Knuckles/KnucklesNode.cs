using OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Knuckles;

public partial class KnucklesNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode);
        PlayerLogic.SetDependencies(new BaseDependencyGenerator(PlayerLogic, PlayerLogic.Data));
        SpriteNode.SpriteLogic = new KnucklesSpriteLogic(PlayerLogic.Data, SpriteNode);
        
        base._EnterTree();
    }
}
