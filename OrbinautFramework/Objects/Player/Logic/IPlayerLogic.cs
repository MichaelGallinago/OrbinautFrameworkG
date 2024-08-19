using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.PlayerActions;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerLogic : IRecorderStorage
{
    ActionFsm.States Action { get; set; }
    TileCollider TileCollider { get; }
    
    Damage Damage { get; }
    Landing Landing { get; }
    DataUtilities DataUtilities { get; }
    public ObjectInteraction ObjectInteraction { get; }

    void Respawn();
    void Hurt(float positionX = 0f) => Damage.Hurt(positionX);
    void Kill() => Damage.Kill();
    void Land() => Landing.Land();
    void ResetState() => DataUtilities.ResetState();
    void ResetMusic() => DataUtilities.ResetMusic();
    void ResetGravity() => DataUtilities.ResetGravity();
    void ClearPush(object target) => ObjectInteraction.ClearPush(target);
    bool CheckPushCollision(IPlayer player) => ObjectInteraction.CheckPushCollision(player);
    
    bool CheckSolidCollision(SolidBox solidBox, Constants.CollisionSensor type)
    {
        return ObjectInteraction.CheckSolidCollision(solidBox, type);
    }
    
    void ActSolid(ISolid target, Constants.SolidType type, bool isFullRoutine = true)
    {
        ObjectInteraction.ActSolid(target, type, isFullRoutine);
    }
}
