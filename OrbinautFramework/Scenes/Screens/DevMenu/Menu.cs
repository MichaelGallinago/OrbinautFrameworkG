using Godot;
using OrbinautFramework3.Framework.InputModule;
using OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class Menu : VBoxContainer
{
    [Export] private OptionStorage _optionStorage;
    
    [Signal] public delegate void SelectedEventHandler(Menu menu);
    
    private Option _currentOption;
    
    public override void _Ready() => _currentOption = _optionStorage.Current;
    
    public void Process(Buttons press)
    {
        if (press.Down)
        {
            _currentOption = _optionStorage.Next;
        }
        else if (press.Up)
        {
            _currentOption = _optionStorage.Previous;
        }

        if (press.A || press.Start) _currentOption.PressSelect();
        else if (press.X) _currentOption.PressX();
        else if (press.Y) _currentOption.PressY();
        else if (press.Right) _currentOption.PressRight();
        else if (press.Left) _currentOption.PressLeft();
    }

    private void Select()
    {
        EmitSignal(SignalName.Selected, this);
        _currentOption = _optionStorage.Current;
    }
}
