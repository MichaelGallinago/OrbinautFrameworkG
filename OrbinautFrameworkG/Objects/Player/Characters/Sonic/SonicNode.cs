using OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Sonic;

public partial class SonicNode : PlayerNode
{
    public override void _EnterTree()
    {
        PlayerLogic = new PlayerLogic(this, SpriteNode);
        PlayerLogic.SetDependencies(new BaseDependencyGenerator(PlayerLogic, PlayerLogic.Data));
        SpriteNode.SpriteLogic = new SonicSpriteLogic(PlayerLogic.Data, SpriteNode);
        
        base._EnterTree();
    }
}
