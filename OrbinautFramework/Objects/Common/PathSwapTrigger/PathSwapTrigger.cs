using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

public abstract partial class PathSwapTrigger : Trigger
{
    [Export] private Sprite2D _sprite;
    [Export] private bool _isGroundOnly;

    protected Vector2 Borders;

    public override void _Ready()
    {
        if (_sprite == null) return;
        float size = _sprite.Texture.GetSize().Y * Math.Abs(Scale.Y) / 2f;
        Borders = new Vector2(-size, size);
    }

    public override void _Process(double delta)
    {
        foreach (IPlayer player in Scene.Instance.Players.Values)
        {
            if (_isGroundOnly && !player.Data.Movement.IsGrounded) continue;
            if (!player.Data.Collision.IsObjectInteractionEnabled) continue;
            
            IPlayerNode node = player.Data.Node;
            Constants.TileLayers? layer = GetTileLayer((Vector2I)node.Position, (Vector2I)node.PreviousPosition);
            
            if (layer == null) return;
            
            player.Data.Collision.TileLayer = (Constants.TileLayers)layer;
        }
    }
    
    protected abstract Constants.TileLayers? GetTileLayer(Vector2I position, Vector2I previousPosition);
}
