using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public static class SharedData
{
    // Game settings
    public static ushort GameWidth { get; set; } = 400;
    public static ushort GameHeight { get; set; } = 224;
    //public static Room StartRoom { get; set; } = rm_devmenu; // TODO: add StartRoom
    public static bool DevMode { get; set; } = true;
    public static bool ShowSplash { get; set; } = false;
	
    // Originals differences
    public static PlayerConstants.PhysicsType PlayerPhysics { get; set; } = PlayerConstants.PhysicsType.S2;
    public static bool SpinDash { get; set; } = true;
    public static bool PeelOut { get; set; } = true;
    public static bool DropDash { get; set; } = true;
    public static bool DoubleSpin { get; set; } = true;
    public static bool CDTimer { get; set; } = false;
    public static bool CDCamera { get; set; } = false;
    public static bool CDTileFixes { get; set; } = true;
	
    // Orbinaut improvements
    public static byte RotationMode { get; set; } = 0;
    public static bool NoRollLock { get; set; } = false;
    public static bool NoSpeedCap { get; set; } = true;
    public static bool FixJumpSize { get; set; } = true;
    public static bool FixDashRelease { get; set; } = true;
    public static bool FlightCancel { get; set; } = true;
    public static bool BetterSolidCollision { get; set; } = false;
    public static bool NoCameraCap { get; set; } = false;
    
    // Common global variables
    public static byte CurrentSaveSlot { get; set; } = 0;
    public static float MusicVolume { get; set; } = 0.5f;
    public static float SoundVolume { get; set; } = 0.5f;
    public static byte DebugCollision { get; set; } = 0;
    //public static checkpoint_data { get; set; } = []; // TODO: add checkpoint_data, giant_ring_data & ds_giant_rings
    //public static giant_ring_data { get; set; } = [];
    //public static ds_giant_rings { get; set; } = ds_list_create();
    public static bool PlayerEditMode { get; set; } = false;
    public static PlayerConstants.Type PlayerMain { get; set; } = PlayerConstants.Type.Sonic;
    public static PlayerConstants.Type PlayerCpu { get; set; } = PlayerConstants.Type.None;
    public static byte StageId { get; set; } = 0;
    public static byte ContinueCount { get; set; } = 3;
    public static byte EmeraldCount { get; set; } = 7;
    public static byte SavedLives { get; set; } = 3;
    public static long SavedScore { get; set; } = 0;
    public static byte SavedRings { get; set; } = 0;
    public static Constants.Barrier SavedBarrier { get; set; } = Constants.Barrier.None;
}