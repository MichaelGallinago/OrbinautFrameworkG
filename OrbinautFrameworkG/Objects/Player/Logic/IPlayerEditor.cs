using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerEditor : IEditor, IPlayerNodePosition
{
    Constants.Direction IEditor.Facing => Data.Visual.Facing;
    IInputContainer IEditor.Input => Data.Input;
}
