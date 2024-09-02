using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.Settings;

public abstract partial class Setting : Option
{
    [Export] private Label _value;
    
    protected string ValueText { set => _value.Text = value; }
}
