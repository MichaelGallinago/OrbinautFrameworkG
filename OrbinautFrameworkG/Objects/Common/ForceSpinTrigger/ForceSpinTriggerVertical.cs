using Godot;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Common.ForceSpinTrigger;

public partial class ForceSpinTriggerVertical : ForceSpinTrigger
{
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }
    
    protected override bool CheckForcePlayerSpin(IPlayer player)
    {
        var playerPosition = (Vector2I)player.Data.Node.Position;
		
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return false;
        return (int)player.Data.Node.PreviousPosition.Y >= Position.Y != playerPosition.Y >= Position.Y;
    }
}
