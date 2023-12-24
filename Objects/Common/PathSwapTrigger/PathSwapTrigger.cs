using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

using Player;

public abstract partial class PathSwapTrigger : CommonObject
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

    protected override void Update(float processSpeed)
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
