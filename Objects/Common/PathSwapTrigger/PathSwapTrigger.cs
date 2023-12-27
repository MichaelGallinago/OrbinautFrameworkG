using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.CommonObject;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public abstract partial class PathSwapTrigger : BaseObject
{
    [Export] protected Sprite2D Sprite;
    [Export] protected bool IsGroundOnly;

    protected Vector2 Borders;

    protected PathSwapTrigger() => Visible = false;

    public override void _Ready()
    {
        if (Sprite == null) return;
        float size = Sprite.Texture.GetSize().Y * Math.Abs(Scale.Y) / 2f;
        Borders = new Vector2(-size, size);
    }

    public override void _Process(double delta)
    {
        Visible = SharedData.DebugCollision > 0;

        foreach (Player player in Player.Players)
        {
            if (IsGroundOnly && !player.IsGrounded || player.IsEditMode) continue;
            UpdatePlayerTileLayer(player);
        }
    }
    
    protected abstract void UpdatePlayerTileLayer(Player player);
}
