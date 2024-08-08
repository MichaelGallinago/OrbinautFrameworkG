using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public partial class PathSwapTriggerHorizontal : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerLeft = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerRight = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }

    protected override void UpdatePlayerTileLayer(PlayerNode playerNode)
    {
        var playerPosition = (Vector2I)playerNode.Position;
        
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return;
        
        var previousPositionX = (int)playerNode.PreviousPosition.X;
        if (previousPositionX < Position.X && playerPosition.X >= Position.X)
        {
            playerNode.TileLayer = _layerRight;
        }
        else if (previousPositionX >= Position.X && playerPosition.X < Position.X)
        {
            playerNode.TileLayer = _layerLeft;
        }
    }
}
