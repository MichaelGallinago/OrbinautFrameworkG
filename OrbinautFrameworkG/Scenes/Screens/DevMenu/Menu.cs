using Godot;
using OrbinautFrameworkG.Framework.InputModule;
using OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu;

public partial class Menu : VBoxContainer
{
    [Signal] public delegate void SelectedEventHandler(Menu menu);
    
    [Export] private OptionStorage _optionStorage;
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
    
    public virtual void OnExit() {}

    private void Select()
    {
        EmitSignal(SignalName.Selected, this);
        _currentOption = _optionStorage.First;
    }
}
