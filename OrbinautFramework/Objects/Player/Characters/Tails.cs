using Godot;

namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Tails : PlayerNode//TODO: ICarrier
{
    [Export] private Sprite.Tail _tail;
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        _tail.Animate(this);
    }
}