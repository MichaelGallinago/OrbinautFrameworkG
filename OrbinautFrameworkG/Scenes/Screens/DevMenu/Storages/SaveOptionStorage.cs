using Godot;

namespace OrbinautFrameworkG.Scenes.Screens.DevMenu;

public partial class SaveOptionStorage : OptionStorage
{
	[Export] private PackedScene _defaultScene;
	
	public override void _EnterTree()
	{
		Prefabs.SaveOption[] options = FilterNodes<Prefabs.SaveOption>();
		for (byte i = 0; i < options.Length; i++)
		{
			options[i].SetSlot(i, _defaultScene);
		}
	}
}
