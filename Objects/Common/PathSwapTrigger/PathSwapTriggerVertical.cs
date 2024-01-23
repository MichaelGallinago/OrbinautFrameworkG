using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public partial class PathSwapTriggerVertical : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerAbove;
    [Export] private Constants.TileLayers _layerBelow;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += new Vector2(Position.X, Position.X);
    }

    protected override void UpdatePlayerTileLayer(Player player)
    {
        var playerPosition = (Vector2I)player.Position;
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return;
            
        var previousPosition = (Vector2I)player.PreviousPosition;
        if (previousPosition.Y < Position.Y && playerPosition.Y >= Position.Y)
        {
            player.TileLayer = _layerBelow;
        }
        else if (previousPosition.Y >= Position.X && playerPosition.X < Position.X)
        {
            player.TileLayer = _layerAbove;
        }
    }
}
