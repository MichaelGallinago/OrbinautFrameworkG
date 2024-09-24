using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.ObjectBase;

namespace OrbinautFrameworkG.Objects.Player;

public interface IEditor : IPosition
{
    Constants.Direction Facing { get; }
    IInputContainer Input { get; }
    
    void OnEnableDebugMode();
    void OnDisableDebugMode();
}
