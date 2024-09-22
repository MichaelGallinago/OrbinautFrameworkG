using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class SaveOption : Option
{
    [Export] private Label _label;
    [Export] private PackedScene _defaultScene;

    public void SetSlot(byte slot) => _label.Text = slot switch
    {
        0 => "NO SAVE",
        _ => SaveData.IsSaveExists(slot) ? $"SAVED GAME {slot}" : $"SAVE {slot}"
    };
}
