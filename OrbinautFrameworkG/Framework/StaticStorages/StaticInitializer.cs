using Godot;

namespace OrbinautFrameworkG.Framework.StaticStorages;

public partial class StaticInitializer : Node
{
	public StaticInitializer()
	{
		Settings.IsInitialized = true;
		SaveData.IsInitialized = true;
		PhysicsServer2D.SetActive(false);
		PhysicsServer3D.SetActive(false);
		QueueFree();
	}
}
