using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Actions;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerLogic : IRecorderStorage, IPlayerActionStorage
{
    TileCollider TileCollider { get; }
    ControlType ControlType { get; }
    
    Damage Damage { get; }
    Landing Landing { get; }
    DataUtilities DataUtilities { get; }
    
    protected ObjectInteraction ObjectInteraction { get; } //TODO: check encapsulation?
    
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
    
    bool CheckSolidCollision(SolidBox solidBox, Constants.CollisionSensor type)
    {
        return ObjectInteraction.CheckSolidCollision(solidBox, type);
    }
    
    void ActSolid(ISolid target, Constants.SolidType type, 
        Constants.AttachType attachType = Constants.AttachType.Default)
    {
        ObjectInteraction.ActSolid(target, type, attachType);
    }
}
