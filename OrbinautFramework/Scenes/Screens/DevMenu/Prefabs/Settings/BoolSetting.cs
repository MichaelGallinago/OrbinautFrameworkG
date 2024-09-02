namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.Settings;

public partial class BoolSetting : Setting
{
    public bool Value { set => ValueText = value.ToString(); }
}
