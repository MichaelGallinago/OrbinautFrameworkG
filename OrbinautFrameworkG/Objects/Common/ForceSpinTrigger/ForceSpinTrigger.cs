using System;
using Godot;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Objects.Player;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Common.ForceSpinTrigger;

public abstract partial class ForceSpinTrigger : Trigger
{
    [Export] private Sprite2D _sprite;

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
            if (!player.Data.State.IsObjectInteractable()) continue;
            if (!CheckForcePlayerSpin(player)) continue;
            
            player.Data.Movement.IsForcedRoll = !player.Data.Movement.IsForcedRoll;
            player.Action = ActionFsm.States.Default;
            
            player.ResetGravity();
        }
    }
    
    protected abstract bool CheckForcePlayerSpin(IPlayer playerNode);
}
