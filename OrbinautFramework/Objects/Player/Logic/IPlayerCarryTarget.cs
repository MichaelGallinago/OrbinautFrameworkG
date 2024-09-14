using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Physics.Collisions;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerCarryTarget : ICarryTarget, IPlayerLogic, IPlayerPosition
{
    Constants.Direction ICarryTarget.Facing
    {
        get => Data.Visual.Facing;
        set => Data.Visual.Facing = value;
    }
    
    Vector2 ICarryTarget.Velocity 
    {
        get => Data.Movement.Velocity;
        set => Data.Movement.Velocity = value;
    }
    
    Vector2 ICarryTarget.Scale
    {
        get => Data.Visual.Scale;
        set => Data.Visual.Scale = value;
    }
    
    bool ICarryTarget.TryFree(out float cooldown)
    {
        if (Action != ActionFsm.States.Carried)
        {
            cooldown = 60f;
            return true;
        }
        
        if (Data.Input.Press.Aby)
        {
            Action = ActionFsm.States.Jump;
            cooldown = 18f;
            return true;
        }
        
        cooldown = 0f;
        return false;
    }
    
    void ICarryTarget.OnFree()
    {
        if (Action == ActionFsm.States.Jump) return;
        Action = ActionFsm.States.Default;
    }
    
    void ICarryTarget.Collide() => new Air(Data, this).Collide();
}
