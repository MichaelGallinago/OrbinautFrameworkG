using Godot;

namespace OrbinautFrameworkG.Framework.StaticStorages;

public partial class StaticInitializer : Node
{
	public override void _Ready()
	{
		PhysicsServer2D.SetActive(false);
		PhysicsServer3D.SetActive(false);
		
		Settings.IsInitialized = true;
		SaveData.IsInitialized = true;
		ConfigUtilities.Load();
		
		QueueFree();
	}
}
