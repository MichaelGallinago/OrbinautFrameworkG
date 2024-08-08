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
    
    protected override bool CheckForcePlayerSpin(PlayerNode playerNode)
    {
        var playerPosition = (Vector2I)playerNode.Position;
		
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return false;
        return (int)playerNode.PreviousPosition.Y >= Position.Y != playerPosition.Y >= Position.Y;
    }
}
