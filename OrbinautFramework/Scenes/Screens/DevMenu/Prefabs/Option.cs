using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

[GlobalClass]
public partial class Option : HBoxContainer
{
    [Export] private Label _leftArrow;
    [Export] private Label _rightArrow;
    
    [Signal] public delegate void PressedRightEventHandler();
    [Signal] public delegate void PressedLeftEventHandler();
    [Signal] public delegate void PressedSelectEventHandler();
    [Signal] public delegate void PressedXEventHandler();
    [Signal] public delegate void PressedYEventHandler();
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            _leftArrow.Visible = value;
            _rightArrow.Visible = value;
        }
    }
    private bool _isSelected;

    public void PressRight() => EmitSignal(SignalName.PressedRight);
    public void PressLeft() => EmitSignal(SignalName.PressedLeft);
    public void PressSelect() => EmitSignal(SignalName.PressedSelect);
    public void PressX() => EmitSignal(SignalName.PressedX);
    public void PressY() => EmitSignal(SignalName.PressedY);
}
