using OrbinautFrameworkG.Framework;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public interface IPlayerEditor : IEditor, IPlayerPosition
{
    Constants.Direction IEditor.Facing => Data.Visual.Facing;
    IInputContainer IEditor.Input => Data.Input;
}
