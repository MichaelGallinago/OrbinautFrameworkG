using Godot;

namespace OrbinautFramework3.Objects.Common.ForceSpinTrigger;

using Player;

public partial class ForceSpinTriggerHorizontal : ForceSpinTrigger
{
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }
    
    protected override bool CheckForcePlayerSpin(PlayerNode playerNode)
    {
        var playerPosition = (Vector2I)playerNode.Position;
        
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return false;
        return (int)playerNode.PreviousPosition.X >= Position.X != playerPosition.X >= Position.X;
    }
}
