using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Common.PathSwapTrigger;

public partial class PathSwapTriggerVertical : PathSwapTrigger
{
    [Export] private Constants.TileLayers _layerAbove = Constants.TileLayers.Secondary;
    [Export] private Constants.TileLayers _layerBelow = Constants.TileLayers.Main;
    
    public override void _Ready()
    {
        base._Ready();
        Borders += Vector2.One * Position.X;
    }
    
    protected override Constants.TileLayers? GetTileLayer(Vector2I position, Vector2I previousPosition)
    {
        if (position.X < Borders.X || position.X >= Borders.Y) return null;

        if (previousPosition.Y < Position.Y && position.Y >= Position.Y) return _layerBelow;
        if (previousPosition.Y >= Position.Y && position.Y < Position.Y) return _layerAbove;
        
        return null;
    }
}
