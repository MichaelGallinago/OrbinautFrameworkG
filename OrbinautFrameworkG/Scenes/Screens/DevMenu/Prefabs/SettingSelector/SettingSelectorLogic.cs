using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu.Prefabs.SettingSelector;

[GlobalClass] 
public abstract partial class SettingSelectorLogic : Resource
{
    public abstract string GetText();
    public abstract void OnLeftPressed();
    public abstract void OnRightPressed();
    public virtual void OnSelectPressed() {}
}
