namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class SaveOptionStorage : OptionStorage
{
	public override void _EnterTree()
	{
		SaveOption[] options = FilterNodes<SaveOption>();
		for (byte i = 0; i < options.Length; i++)
		{
			options[i].SetSlot(i);
		}
	}
}
