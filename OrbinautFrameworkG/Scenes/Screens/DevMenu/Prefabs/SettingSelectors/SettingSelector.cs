using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelectors;

public partial class SettingSelector : Option
{
    [Export] private Label _value;
    [Export] private SettingSelectorLogic _logic;

    public override void _Ready() => Update();

    public override void PressLeft()
    {
        base.PressLeft();
        _logic.OnLeftPressed();
        Update();
    }
    
    public override void PressRight()
    {
        base.PressLeft();
        _logic.OnRightPressed();
        Update();
    }
    
    public override void PressSelect()
    {
        base.PressLeft();
        _logic.OnSelectPressed();
        Update();
    }
    
    private void Update() => _value.Text = _logic.GetText();
}
