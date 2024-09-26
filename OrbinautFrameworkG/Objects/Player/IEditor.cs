using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player;

public interface IEditor : IPosition
{
    Constants.Direction Facing { get; }
    IInputContainer Input { get; }
    
    void OnEnableDebugMode();
    void OnDisableDebugMode();
}
