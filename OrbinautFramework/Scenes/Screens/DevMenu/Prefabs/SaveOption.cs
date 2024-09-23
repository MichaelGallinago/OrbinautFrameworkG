using Godot;
using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class SaveOption : Option
{
    [Signal] public delegate void SelectedSaveEventHandler(PackedScene scene, byte slot);
    
    [Export] private Label _label;
    private PackedScene _defaultScene;
    
    private byte _slot;
    
    public void SetSlot(byte slot, PackedScene defaultScene)
    {
        _slot = slot;
        _defaultScene = defaultScene;
        _label.Text = slot switch
        {
            0 => "NO SAVE",
            _ => SaveData.IsSaveExists(slot) ? $"SAVED GAME {slot}" : $"SAVE {slot}"
        };
    }

    public override void PressSelect()
    {
        base.PressSelect();
        EmitSignal(SignalName.SelectedSave, _defaultScene, _slot);
    }
}
