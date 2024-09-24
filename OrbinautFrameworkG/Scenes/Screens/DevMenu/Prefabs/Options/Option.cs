using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs;

[GlobalClass]
public partial class Option : HBoxContainer
{
    [Signal] public delegate void PressedRightEventHandler();
    [Signal] public delegate void PressedLeftEventHandler();
    [Signal] public delegate void PressedSelectEventHandler();
    [Signal] public delegate void PressedXEventHandler();
    [Signal] public delegate void PressedYEventHandler();
    
    [Export] private Label _leftArrow;
    [Export] private Label _rightArrow;
    
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

    public virtual void PressRight() => EmitSignal(SignalName.PressedRight);
    public virtual void PressLeft() => EmitSignal(SignalName.PressedLeft);
    public virtual void PressSelect() => EmitSignal(SignalName.PressedSelect);
    public virtual void PressX() => EmitSignal(SignalName.PressedX);
    public virtual void PressY() => EmitSignal(SignalName.PressedY);
}
