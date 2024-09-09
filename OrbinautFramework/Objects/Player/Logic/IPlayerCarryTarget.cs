using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerCarryTarget : ICarryTarget, IPlayerDataStorage, IPlayerActionStorage
{
    Constants.Direction ICarryTarget.Facing
    {
        get => Data.Visual.Facing;
        set => Data.Visual.Facing = value;
    }
    
    Vector2 ICarryTarget.Position 
    {
        get => Data.Node.Position;
        set => Data.Node.Position = value;
    }
    
    Vector2 ICarryTarget.Velocity 
    {
        get => Data.Movement.Velocity;
        set => Data.Movement.Velocity.Vector = value;
    }
    
    Vector2 ICarryTarget.Scale
    {
        get => Data.Node.Scale;
        set => Data.Node.Scale = value;
    }

    bool ICarryTarget.IsFree => Action != ActionFsm.States.Carried;
    
    void ICarryTarget.OnFree() => Action = ActionFsm.States.Default;
}
