using System;
using Godot;
using JetBrains.Annotations;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Scenes;
using Scene = OrbinautFramework3.Scenes.Scene;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public abstract partial class PathSwapTrigger : Trigger
{
    [Export] private Sprite2D _sprite;
    [Export] private bool _isGroundOnly;
    
    [UsedImplicitly] private IScene _scene;

    protected Vector2 Borders;

    public override void _Ready()
    {
        if (_sprite == null) return;
        float size = _sprite.Texture.GetSize().Y * Math.Abs(Scale.Y) / 2f;
        Borders = new Vector2(-size, size);
    }

    public override void _Process(double delta)
    {
        foreach (Player player in _scene.Players.Values)
        {
            if (_isGroundOnly && !player.IsGrounded || !player.IsObjectInteractionEnabled) continue;
            UpdatePlayerTileLayer(player);
        }
    }
    
    protected abstract void UpdatePlayerTileLayer(Player player);
}
