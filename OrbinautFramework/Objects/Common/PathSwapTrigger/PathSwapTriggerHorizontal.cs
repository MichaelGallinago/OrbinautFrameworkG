using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

public partial class PathSwapTriggerHorizontal : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerLeft = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerRight = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }

    protected override void UpdatePlayerTileLayer(PlayerData player)
    {
        var playerPosition = (Vector2I)player.Node.Position;
        
        if (playerPosition.Y < Borders.X || playerPosition.Y >= Borders.Y) return;
        
        var previousPositionX = (int)player.Node.PreviousPosition.X;
        if (previousPositionX < Position.X && playerPosition.X >= Position.X)
        {
            player.Collision.TileLayer = _layerRight;
        }
        else if (previousPositionX >= Position.X && playerPosition.X < Position.X)
        {
            player.Collision.TileLayer = _layerLeft;
        }
    }
}
