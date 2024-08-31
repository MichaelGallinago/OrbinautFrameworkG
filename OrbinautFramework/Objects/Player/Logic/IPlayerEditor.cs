using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;

namespace OrbinautFramework3.Objects.Player.Logic;

public interface IPlayerEditor : IEditor, IPlayerDataStorage
{
    Constants.Direction IEditor.Facing => Data.Visual.Facing;
    IInputContainer IEditor.Input => Data.Input;

    Vector2 IPosition.Position
    {
        get => Data.Node.Position; 
        set => Data.Node.Position = value;
    }
}
