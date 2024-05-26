using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public partial class PathSwapTriggerVertical : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerAbove = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerBelow = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }

    protected override void UpdatePlayerTileLayer(Player player)
    {
        var playerPosition = (Vector2I)player.Position;
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return;
            
        var previousPositionY = (int)player.PreviousPosition.Y;
        if (previousPositionY < Position.Y && playerPosition.Y >= Position.Y)
        {
            player.TileLayer = _layerBelow;
        }
        else if (previousPositionY >= Position.X && playerPosition.X < Position.X)
        {
            player.TileLayer = _layerAbove;
        }
    }
}
