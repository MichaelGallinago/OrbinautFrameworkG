namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class BoolSetting : Setting
{
    public bool Value { set => ValueText = value.ToString(); }
}
