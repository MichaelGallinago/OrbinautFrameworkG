using Godot;
using OrbinautFrameworkG.Framework;

namespace OrbinautFrameworkG.Objects.Common.PathSwapTrigger;

public partial class PathSwapTriggerHorizontal : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerLeft = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerRight = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.Y;
    }

    protected override Constants.TileLayers? GetTileLayer(Vector2I position, Vector2I previousPosition)
    {
        if (position.Y < Borders.X || position.Y >= Borders.Y) return null;
        
        if (previousPosition.X < Position.X && position.X >= Position.X) return _layerRight;
        if (previousPosition.X >= Position.X && position.X < Position.X) return _layerLeft;
        
        return null;
    }
}
