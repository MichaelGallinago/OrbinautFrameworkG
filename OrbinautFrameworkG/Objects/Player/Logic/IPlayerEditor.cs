using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerEditor : IEditor, IPlayerPosition
{
    Constants.Direction IEditor.Facing => Data.Visual.Facing;
    IInputContainer IEditor.Input => Data.Input;
}
