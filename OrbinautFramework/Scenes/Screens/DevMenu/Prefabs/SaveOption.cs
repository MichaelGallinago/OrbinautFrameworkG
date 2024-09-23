using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class SaveOption : Option
{
    [Signal] public delegate void SelectedSaveEventHandler(PackedScene scene, byte slot);
    
    [Export] private Label _label;
    [Export] private PackedScene _defaultScene;
    
    private byte _slot;
    
    public void SetSlot(byte slot)
    {
        _slot = slot;
        _label.Text = slot switch
        {
            0 => "NO SAVE",
            _ => SaveData.IsSaveExists(slot) ? $"SAVED GAME {slot}" : $"SAVE {slot}"
        };
    }

    public override void PressSelect()
    {
        base.PressSelect();
        EmitSignal(SignalName.SelectedSave, _slot);
    }
}
