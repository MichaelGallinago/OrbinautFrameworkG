using Godot;
using OrbinautFramework3.Objects.Common.GiantRing;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Framework;

public static class SharedData
{
    // Game settings
    public static Vector2I ViewSize { get; set; } = new(400, 224);
    //public static Room StartRoom { get; set; } = rm_devmenu; // TODO: add StartRoom
    public static bool DevMode { get; set; } = true;
    public static bool ShowSplash { get; set; } = false;
	
    // Originals differences
    public static PhysicsTypes PlayerPhysics { get; set; } = PhysicsTypes.S2;
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
    public static byte? CurrentSaveSlot { get; set; } = 0;
    public static float MusicVolume { get; set; } = 0.5f;
    public static float SoundVolume { get; set; } = 0.5f;
    public static byte DebugCollision { get; set; } = 0;
	public static CheckpointData CheckpointData { get; set; }
    public static GiantRingData GiantRingData { get; set; }
    // TODO: ds_giant_rings
    //public static ds_giant_rings { get; set; } = ds_list_create();
    public static bool PlayerEditMode { get; set; } = false;
    public static Types PlayerType { get; set; } = Types.Sonic;
    public static Types PlayerTypeCpu { get; set; } = Types.Tails;
    public static byte StageId { get; set; } = 0;
    public static byte ContinueCount { get; set; } = 3;
    public static byte EmeraldCount { get; set; } = 7;
    public static byte SavedLives { get; set; } = 3;
    public static uint SavedScore { get; set; } = 0;
    public static byte SavedRings { get; set; } = 0;
    public static Barrier.Types SavedBarrier { get; set; } = Barrier.Types.None;
}