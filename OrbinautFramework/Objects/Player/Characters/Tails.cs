using Godot;

namespace OrbinautFramework3.Objects.Player.Characters;

public partial class Tails : PlayerNode//, ICarrier
{
    [Export] private Tail _tail;
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        _tail.Animate(this);
    }
}