using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player;

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
    public static CommonScene CurrentScene { get; set; }
    public static bool PlayerEditMode { get; set; }
    public static bool DeveloperMode { get; set; }
    public static bool IsPaused { get; set; }
    public static double Time { get; set; }

    static FrameworkData()
    {
        UpdateAnimations = true;
        UpdateEffects = true;
        UpdateObjects = true;
        UpdateTimer = true;
        AllowPause = true;
        TilesData = CollisionUtilities.LoadTileDataBinary(
            "angles_tsz", "heights_tsz", "widths_tsz");
        

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
    }
}
