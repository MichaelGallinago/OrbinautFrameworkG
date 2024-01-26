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
		
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return false;
        return (int)player.PreviousPosition.X >= Position.X != playerPosition.X >= Position.X;
    }
}
