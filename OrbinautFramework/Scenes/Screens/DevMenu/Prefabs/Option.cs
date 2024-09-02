using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class Option : HBoxContainer
{
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
}
