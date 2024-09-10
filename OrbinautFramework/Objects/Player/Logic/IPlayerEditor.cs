using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerEditor : IEditor, IPlayerPosition
{
    Constants.Direction IEditor.Facing => Data.Visual.Facing;
    IInputContainer IEditor.Input => Data.Input;
}
