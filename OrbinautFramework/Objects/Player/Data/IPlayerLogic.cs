using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.PlayerActions;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerLogic
{
    Recorder Recorder { get; }
    ActionFsm.States Action { get; set; }
    
    Damage Damage { get; }
    Landing Landing { get; }
    DataUtilities DataUtilities { get; }
    
    void Hurt(float positionX = 0f) => Damage.Hurt(positionX);
    void Kill() => Damage.Kill();
    void Land() => Landing.Land();
    void ResetState() => DataUtilities.ResetState();
    void ResetMusic() => DataUtilities.ResetMusic();
    void ResetGravity() => DataUtilities.ResetGravity();
}
