using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public interface IInteractive : ITypeDelegate, IPosition
{
    protected bool IsInteract { get; set; }
    public HitBox HitBox { get; }
    
    public bool CheckCollision(IInteractive target, Vector2I position, bool isExtra = false)
    {
        if (!IsInteract || !target.IsInteract) return false;
        
        if (!HitBox.CheckCollision(target.HitBox, (Vector2I)target.Position, position, isExtra)) return false;
        
        // Objects should no longer interact with any other object this frame
        IsInteract = false;
        target.IsInteract = false;
        return true;
    }
    
    public bool CheckPlayerCollision(PlayerData player, Vector2I position, bool isExtraHitBox = false)
    {
        return player.State == PlayerStates.Control && CheckCollision(player.Node, position, isExtraHitBox);
    }
    
    void ITypeDelegate.Invoke() => IsInteract = true;
}