using Godot;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.ForceSpinTrigger;

public partial class ForceSpinTriggerHorizontal : ForceSpinTrigger
{
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }

    protected override bool CheckForcePlayerSpin(PlayerData player)
    {
        var playerPosition = (Vector2I)player.Node.Position;
        
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return false;
        return (int)player.Node.PreviousPosition.X >= Position.X != playerPosition.X >= Position.X;
    }
}
