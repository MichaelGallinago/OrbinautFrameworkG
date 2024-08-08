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

    protected override void UpdatePlayerTileLayer(PlayerNode playerNode)
    {
        var playerPosition = (Vector2I)playerNode.Position;
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return;
            
        var previousPositionY = (int)playerNode.PreviousPosition.Y;
        if (previousPositionY < Position.Y && playerPosition.Y >= Position.Y)
        {
            playerNode.TileLayer = _layerBelow;
        }
        else if (previousPositionY >= Position.X && playerPosition.X < Position.X)
        {
            playerNode.TileLayer = _layerAbove;
        }
    }
}
