using Godot;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Tails : PlayerNode//TODO: ICarrier
{
    [Export] private Tail _tail;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _tail.Animate(PlayerLogic);
    }
}