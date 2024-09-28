using Godot;

namespace OrbinautFrameworkG.Framework.StaticStorages;

public partial class StaticStorageInitializer : Node
{
	public StaticStorageInitializer()
	{
		Settings.IsInitialized = true;
		SaveData.IsInitialized = true;
		QueueFree();
	}
}
