using Godot;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.SettingButtons;

[GlobalClass] 
public abstract partial class SettingSelectorLogic : Resource
{
    public abstract string GetText();
    public abstract void OnLeftPressed();
    public abstract void OnRightPressed();
}
