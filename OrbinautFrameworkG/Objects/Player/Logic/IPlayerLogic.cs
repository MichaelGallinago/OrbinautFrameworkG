using Godot;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Framework.Tiles;
using OrbinautFrameworkG.Objects.Player.Actions;
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerLogic : IRecorderStorage, IPlayerActionStorage
{
    TileCollider TileCollider { get; }
    ControlType ControlType { get; }
    DataUtilities DataUtilities { get; }
    
    protected Landing Landing { get; }
    protected ref Damage Damage { get; }
    protected ref ObjectInteraction ObjectInteraction { get; }
    
    void Init();
    void Respawn() => Damage.Respawn();
    void Hurt(float positionX) => Damage.Hurt(positionX);
    void Hurt(float positionX, AudioStream sound) => Damage.Hurt(positionX, sound);
    void Kill() => Damage.Kill();
    void Kill(AudioStream sound) => Damage.Kill(sound);
    void Land() => Landing.Land();
    void ResetData() => DataUtilities.ResetData();
    void ResetMusic() => DataUtilities.ResetMusic();
    void ResetGravity() => DataUtilities.ResetGravity();
    void ClearPush(object target) => ObjectInteraction.ClearPush(target);
    bool CheckPushCollision(IPlayer player) => ObjectInteraction.CheckPushCollision(player);
    
    bool CheckSolidCollision(ISolid target, Constants.CollisionSensor type)
    {
        return ObjectInteraction.CheckSolidCollision(target, type);
    }
    
    void ActSolid(ISolid target, Constants.SolidType type, 
        Constants.AttachType attachType = Constants.AttachType.Default)
    {
        ObjectInteraction.ActSolid(target, type, attachType);
    }
}
