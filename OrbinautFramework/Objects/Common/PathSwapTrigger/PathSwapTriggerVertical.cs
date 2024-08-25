using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

public partial class PathSwapTriggerVertical : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerAbove = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerBelow = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }

    protected override void UpdatePlayerTileLayer(PlayerData player)
    {
        var playerPosition = (Vector2I)player.Node.Position;
        if (playerPosition.X < Borders.X || playerPosition.X >= Borders.Y) return;
            
        var previousPositionY = (int)player.Node.PreviousPosition.Y;
        if (previousPositionY < Position.Y && playerPosition.Y >= Position.Y)
        {
            player.Collision.TileLayer = _layerBelow;
        }
        else if (previousPositionY >= Position.X && playerPosition.X < Position.X)
        {
            player.Collision.TileLayer = _layerAbove;
        }
    }
}
