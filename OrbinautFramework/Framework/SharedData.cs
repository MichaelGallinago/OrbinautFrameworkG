using System;
using Godot;
using OrbinautFramework3.Objects.Common.GiantRing;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Framework;

public static class SharedData
{
	// Default settings. May be overwritten by the config file
    private static Vector2I _viewSize = new(400, 224);
    public static byte WindowScale { get; set; } = 2;
    public static int TargetFps { get; set; } = 165;
    public static bool ShowSplash { get; set; } = false;
    public static float MusicVolume { get; set; } = 0.5f;
    public static float SoundVolume { get; set; } = 0.5f;
    //public static Room StartRoom { get; set; } = rm_devmenu; // TODO: add StartRoom
    public static bool SkipBranding { get; set; } = true;

    // Common
    public static byte StageIndex { get; set; } = 0; // TODO: make stage prefab storage with enum "Stage"
    public static byte PreviousRoomId { get; set; } = 0; // TODO: Replace to room
    public static byte? CurrentSaveSlot { get; set; } = null; // null = no-save slot by default
	
    // Originals differences
    public static bool SpinDash { get; set; } = true;
    public static bool Dash { get; set; } = true;
    public static bool DropDash { get; set; } = true;
    public static bool DoubleSpin { get; set; } = true;
    public static bool CdTimer { get; set; } = false;
    public static bool CdCamera { get; set; } = true;
    public static bool SuperstarsTweaks { get; set; } = true;
	
    // Orbinaut improvements
    public static byte RotationMode { get; set; } = 1;
    public static bool NoRollLock { get; set; } = false;
    public static bool NoSpeedCap { get; set; } = true;
    public static bool FixJumpSize { get; set; } = true;
    public static bool FixDashRelease { get; set; } = true;
    public static bool BetterSolidCollision { get; set; } = false;
    public static bool NoCameraCap { get; set; } = false;
    
    // Common global variables
	public static CheckpointData CheckpointData { get; set; }
    public static GiantRingData GiantRingData { get; set; }
    // TODO: ds_giant_rings
    //public static ds_giant_rings { get; set; } = ds_list_create();
    public static Vector2I LifeRewards { get; set; }
    public static bool IsDebugModeEnabled { get; set; } = false;
    public static int RealPlayerCount { get; set; } = 1;
    public static PlayerNode.Types[] PlayerTypes { get; set; } = [PlayerNode.Types.Sonic]; //[PlayerNode.Types.Sonic, PlayerNode.Types.Tails]; TODO: menu and CPU
    public static byte ContinueCount { get; set; } = 3;
    public static byte EmeraldCount { get; set; } = 7;
    
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

    public enum SensorDebugTypes : byte
    {
	    None, Collision, HitBox, SolidBox
    }
    
    public static void ClearFull()
    {
	    CheckpointData = null;
	    GiantRingData = null;
	    // TODO: ds_giant_rings
	    //ds_list_clear(global.ds_giant_rings);
	    
	    Clear();
    }

    public static void Clear()
    {
	    PlayerShield = ShieldContainer.Types.None;
	    PlayerRings = 0;
	    LifeRewards = Vector2I.Zero;
    }
    
    private static ReadOnlySpan<uint> ComboScoreValues => [10, 100, 200, 500, 1000, 10000];
    public static void IncreaseComboScore(int comboCounter = 0)
    {
	    ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
    }
}
