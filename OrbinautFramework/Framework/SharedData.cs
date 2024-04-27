using System;
using Godot;
using OrbinautFramework3.Objects.Common.GiantRing;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Framework;

public static class SharedData
{
	// Game settings
    private static Vector2I _viewSize = new(428, 240);
    public static byte WindowScale { get; set; } = 2;
    public static int TargetFps { get; set; } = 165;
    public static bool DevMode { get; set; } = true;
    public static bool ShowSplash { get; set; } = false;
    public static float MusicVolume { get; set; } = 0.5f;
    public static float SoundVolume { get; set; } = 0.5f;
    //public static Room StartRoom { get; set; } = rm_devmenu; // TODO: add StartRoom
    public static bool SkipBranding { get; set; } = true;

	
    // Originals differences
    public static PhysicsTypes PlayerPhysics { get; set; } = PhysicsTypes.S2;
    public static CpuBehaviours CpuBehaviour { get; set; } = CpuBehaviours.S3;
    public static bool SpinDash { get; set; } = true;
    public static bool PeelOut { get; set; } = true;
    public static bool DropDash { get; set; } = true;
    public static bool DoubleSpin { get; set; } = true;
    public static bool CdTimer { get; set; } = false;
    public static bool CdCamera { get; set; } = false;
    public static bool CdTileFixes { get; set; } = true;
	
    // Orbinaut improvements
    public static byte RotationMode { get; set; } = 1;
    public static bool NoRollLock { get; set; } = false;
    public static bool NoSpeedCap { get; set; } = true;
    public static bool FixJumpSize { get; set; } = true;
    public static bool FixDashRelease { get; set; } = true;
    public static bool FlightCancel { get; set; } = true;
    public static bool BetterSolidCollision { get; set; } = false;
    public static bool NoCameraCap { get; set; } = false;
    
    // Common global variables
    public static byte? CurrentSaveSlot { get; set; } = 0;
	public static CheckpointData CheckpointData { get; set; }
    public static GiantRingData GiantRingData { get; set; }
    // TODO: ds_giant_rings
    //public static ds_giant_rings { get; set; } = ds_list_create();
    public static bool PlayerEditMode { get; set; } = false;
    public static Types PlayerType { get; set; } = Types.Sonic;
    public static Types PlayerTypeCpu { get; set; } = Types.Tails;
    public static byte StageId { get; set; } = 0;
    public static byte ContinueCount { get; set; }
    public static byte EmeraldCount { get; set; }
    public static byte SavedRings { get; set; }
    
    public static uint ScoreCount { get; set; }
    public static uint PlayerRings { get; set; }
    public static uint LifeCount { get; set; }
    public static ShieldContainer.Types PlayerShield { get; set; } = ShieldContainer.Types.None;

    public static event Action<Vector2I> ViewSizeChanged;
    public static Vector2I ViewSize
    {
	    get => _viewSize;
	    set
	    {
		    ViewSizeChanged?.Invoke(value);
		    _viewSize = value;
	    }
    }
    
    private static SensorDebugTypes _sensorDebugType = SensorDebugTypes.None;
    public static event Action<SensorDebugTypes> SensorDebugToggled;
    public static SensorDebugTypes SensorDebugType 
    { 
	    get => _sensorDebugType;
	    set
	    {
		    if ((value == SensorDebugTypes.None) ^ (_sensorDebugType == SensorDebugTypes.None)) return;
		    SensorDebugToggled?.Invoke(value);
		    _sensorDebugType = value;
	    }
    }
    
    static SharedData() => WindowName = "Orbinaut Framework 3";
    public static string WindowName { set => DisplayServer.WindowSetTitle(value); }

    public enum SensorDebugTypes : byte
    {
	    None, Collision, HitBox, SolidBox
    }
}