using Godot;
using OrbinautFramework3.Objects.Player.Characters.Tails.Tail;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

public partial class TailsNode : PlayerNode//TODO: ICarrier
{
    [Export] private TailNode _tailNode;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _tailNode.Animate(PlayerLogic);
    }
}