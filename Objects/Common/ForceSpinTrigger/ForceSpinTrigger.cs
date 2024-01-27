using System;
using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Common.ForceSpinTrigger;

using Player;

public abstract partial class ForceSpinTrigger : Trigger
{
    [Export] protected Sprite2D Sprite;

    protected Vector2 Borders;
    
    public override void _Ready()
    {
        if (Sprite == null) return;
        float size = Sprite.Texture.GetSize().Y * Math.Abs(Scale.Y) / 2f;
        Borders = new Vector2(-size, size);
    }

    public override void _Process(double delta)
    {
        foreach (Player player in PlayerData.Players)
        {
            var a = CheckForcePlayerSpin(player);
            //GD.Print(a);
            if (player.IsEditMode || !a) continue;
            
            player.IsForcedSpin = !player.IsForcedSpin;
            player.Action = Actions.None;
				
            player.ResetGravity();
        }
    }
    
    protected abstract bool CheckForcePlayerSpin(Player player);
}
