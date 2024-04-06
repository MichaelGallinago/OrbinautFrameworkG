using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Framework;

public static class FrameworkData
{
    public static float ProcessSpeed { get; set; }
    public static TilesData TilesData { get; }
    public static bool UpdateAnimations { get; set; }
    public static bool UpdateEffects { get; set; }
    public static bool UpdateObjects { get; set; }
    public static bool UpdateTimer { get; set; }
    public static bool AllowPause { get; set; }
    public static bool DropDash { get; set; }
    public static PlayerBackupData PlayerBackupData { get; set; }
    public static CommonScene CurrentScene { get; set; }
    public static int RotationMode { get; set; }
    public static uint SavedScore { get; set; }
    public static uint SavedRings { get; set; }
    public static uint SavedLives { get; set; }
    public static Barrier.Types SavedBarrier { get; set; }
    public static bool PlayerEditMode { get; set; }
    public static bool DeveloperMode { get; set; }
    public static bool IsPaused { get; set; }
    public static double Time { get; set; }
    public static ObjectCuller Culler { get; } = new();

    static FrameworkData()
    {
        UpdateAnimations = true;
        UpdateEffects = true;
        UpdateObjects = true;
        UpdateTimer = true;
        AllowPause = true;
        DropDash = true;
        TilesData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        
        RotationMode = 1;

        DeveloperMode = true;
        IsPaused = false;
    }
    
    public static bool IsTimePeriodLooped(float period) => Time % period - ProcessSpeed < 0f;
    public static bool IsTimePeriodLooped(float period, float offset) => (Time + offset) % period - ProcessSpeed < 0f;
    
    public static void UpdateEarly(float processSpeed)
    {
		if (UpdateTimer && !IsPaused)
		{
			Time += processSpeed;
		}
		
		Culler.Cull();
    }
}
