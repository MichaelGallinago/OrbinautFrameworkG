using Godot;

namespace OrbinautFramework3.Objects.Common.ForceSpinTrigger;

using Player;

public partial class ForceSpinTriggerVertical : ForceSpinTrigger
{
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }
    
    protected override bool CheckForcePlayerSpin(Player player)
    {
        var playerPosition = (Vector2I)player.Position;
		
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return false;
        return (int)player.PreviousPosition.Y >= Position.Y != playerPosition.Y >= Position.Y;
    }
}
