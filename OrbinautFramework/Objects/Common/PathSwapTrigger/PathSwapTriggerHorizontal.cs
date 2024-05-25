using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public partial class PathSwapTriggerHorizontal : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerLeft;
    [Export] private Constants.TileLayers _layerRight;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }

    protected override void UpdatePlayerTileLayer(Player player)
    {
        var playerPosition = (Vector2I)player.Position;
        
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return;
        
        var previousPositionX = (int)player.PreviousPosition.X;
        if (previousPositionX < Position.X && playerPosition.X >= Position.X)
        {
            player.TileLayer = _layerRight;
        }
        else if (previousPositionX >= Position.X && playerPosition.X < Position.X)
        {
            player.TileLayer = _layerLeft;
        }
    }
}
