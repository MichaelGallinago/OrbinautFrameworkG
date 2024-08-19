using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player;

public interface IEditor : IPosition
{
    Constants.Direction Facing { get; }
    IInputContainer Input { get; }
    
    void OnEnableEditMode();
    void OnDisableEditMode();
}
