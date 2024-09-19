using System.Collections.Generic;
using System.Linq;
using Godot;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class DevMenu : Control
{
    [Signal] public delegate void PreviousMenuSelectedEventHandler();
    
    [Export] private Menu _currentMenu;
    private PackedScene _nextScene;
    
    private readonly Stack<Menu> _menuStack = new();
    
    public override void _Ready() => _currentMenu.Visible = true;
    
    public override void _Process(double delta)
    {
        InputUtilities.Update();
        Buttons input = InputUtilities.Press.First();
        
        if (_menuStack.Count > 0 && input.B)
        {
            _currentMenu.Visible = false;
            _currentMenu = _menuStack.Pop();
            _currentMenu.Visible = true;
            EmitSignal(SignalName.PreviousMenuSelected);
        }
        
        _currentMenu.Process(input);
    }
    
    public void OnMenuSelected(Menu menu)
    {
        _currentMenu.Visible = false;
        _menuStack.Push(_currentMenu);
        menu.Visible = true;
        _currentMenu = menu;
    }
    
    private void OnSceneSelected(PackedScene scene) => _nextScene = scene;
}
