using Godot;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.ForceSpinTrigger;

public partial class ForceSpinTriggerVertical : ForceSpinTrigger
{
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }
    
    protected override bool CheckForcePlayerSpin(PlayerData player)
    {
        var playerPosition = (Vector2I)player.Node.Position;
		
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return false;
        return (int)player.Node.PreviousPosition.Y >= Position.Y != playerPosition.Y >= Position.Y;
    }
}
