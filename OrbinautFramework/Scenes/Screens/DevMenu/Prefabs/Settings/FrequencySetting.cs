namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.Settings;

public partial class FrequencySetting : Setting
{
    public ushort Value { set => ValueText = value == 0 ? "UNLIMITED" : $"{value}Hz"; }
}
