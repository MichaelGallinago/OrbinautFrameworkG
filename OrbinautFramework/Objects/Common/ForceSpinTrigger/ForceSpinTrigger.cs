using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;

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
        foreach (PlayerData player in Scene.Instance.Players.Values)
        {
            if (player.IsDebugMode || !CheckForcePlayerSpin(player)) continue;
            
            player.Movement.IsForcedSpin = !player.Movement.IsForcedSpin;
            player.State = ActionFsm.States.None;
            
            player.ResetGravity();
        }
    }
    
    protected abstract bool CheckForcePlayerSpin(PlayerData playerNode);
}
