using Godot;
using OrbinautFramework3.Objects.Player.Characters.Logic.Carrier;
using OrbinautFramework3.Objects.Player.Characters.Tails.Tail;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

public partial class TailsNode : PlayerNode
{
    [Export] private TailNode _tailNode;

    public override void _EnterTree()
    {
        var tailsLogic = new TailsLogic(this, SpriteNode);
        tailsLogic.SetDependencies(new CarrierDependencyGenerator(tailsLogic, tailsLogic.Data, tailsLogic.CarryData));
        PlayerLogic = tailsLogic;
        SpriteNode.SpriteLogic = new TailsSpriteLogic(tailsLogic.Data, SpriteNode, tailsLogic.CarryData);
        
        base._EnterTree();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        _tailNode.Animate(PlayerLogic);
    }
}
