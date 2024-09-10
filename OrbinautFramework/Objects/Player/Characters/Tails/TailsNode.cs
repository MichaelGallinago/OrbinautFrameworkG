using Godot;
using OrbinautFramework3.Objects.Player.Characters.Tails.Tail;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

public partial class TailsNode : PlayerNode
{
    [Export] private TailNode _tailNode;

    public override void _EnterTree()
    {
        var tailsLogic = new TailsLogic(this, SpriteNode.PlayerSprite);
        PlayerLogic = tailsLogic;
        
        base._EnterTree();
        
        SpriteNode.SpriteLogic = new TailsSpriteLogic(PlayerLogic.Data, SpriteNode, tailsLogic.CarryData);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        _tailNode.Animate(PlayerLogic);
    }
}
