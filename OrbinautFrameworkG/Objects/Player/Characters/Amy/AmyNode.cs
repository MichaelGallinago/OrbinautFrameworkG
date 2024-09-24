using OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Amy;

public partial class AmyNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode);
        PlayerLogic.SetDependencies(new BaseDependencyGenerator(PlayerLogic, PlayerLogic.Data));
        SpriteNode.SpriteLogic = new AmySpriteLogic(PlayerLogic.Data, SpriteNode);
        
        base._EnterTree();
    }
}
