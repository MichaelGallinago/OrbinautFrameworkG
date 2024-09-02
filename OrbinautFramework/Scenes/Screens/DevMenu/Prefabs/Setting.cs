using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public abstract partial class Setting : HBoxContainer
{
    [Export] private Label _leftArrow;
    [Export] private Label _rightArrow;
    [Export] private Label _value;
    
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
    
    protected string ValueText { set => _value.Text = value; }
}
