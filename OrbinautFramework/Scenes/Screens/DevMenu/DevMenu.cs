using Godot;
using System.Linq;
using System.Collections.Generic;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class DevMenu : Control
{
    [Export] private Menu _currentMenu;
    
    private readonly Stack<Menu> _menuStack = new();
    
    private PackedScene _nextScene;
    
    public override void _Ready() => _currentMenu.Visible = true;
    
    public override void _Process(double delta)
    {
        InputUtilities.Update();
        Buttons input = InputUtilities.Press.First();
        
        if (_menuStack.Count > 0 && input.B)
        {
            _currentMenu.OnExit();
            _currentMenu.Visible = false;
            _currentMenu = _menuStack.Pop();
            _currentMenu.Visible = true;
        }
        
        _currentMenu.Process(input);
    }

    private void OnMenuSelected(Menu menu)
    {
        _currentMenu.Visible = false;
        _menuStack.Push(_currentMenu);
        menu.Visible = true;
        _currentMenu = menu;
    }
    
    private void OnSceneSelected(PackedScene scene) => _nextScene = scene;
    
    private void OnSaveSelected(PackedScene scene, byte slot)
    {
        OnSceneSelected(scene);
        SaveData.Slot = slot;
    }

    private void OnSceneSwitch()
    {
        SaveData.Load();
        if (_nextScene != null)
        {
            GetTree().ChangeSceneToPacked(_nextScene);
            return;
        }
        
        GetTree().ChangeSceneToFile(SaveData.ScenePath);
    }
}
