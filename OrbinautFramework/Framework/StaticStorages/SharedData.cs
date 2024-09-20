using System;
using Godot;
using OrbinautFramework3.Objects.Common.GiantRing;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Framework.StaticStorages;

public static class SharedData
{
    public static bool ShowSplash { get; set; } = false;
    public static bool SkipBranding { get; set; } = true;
    public static byte StageIndex { get; set; } = 0; // TODO: make stage prefab storage with enum "Stage"
    public static byte PreviousRoomId { get; set; } = 0; // TODO: Replace to room
    public static byte? CurrentSaveSlot { get; set; } = null; // null = no-save slot by default
    
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
    public static ShieldContainer.Types[] SavedShields { get; set; }
    public static ushort LifeCount { get; set; }
    
    
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

    public enum SensorDebugTypes : byte { None, Collision, HitBox, SolidBox }
    
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
	    if (SavedShields != null)
	    {
		    for (var i = 0; i < SavedShields.Length; i++)
		    {
			    SavedShields[i] = ShieldContainer.Types.None;
		    }
	    }
	    
	    PlayerRings = 0;
	    LifeRewards = Vector2I.Zero;
    }
    
    private static ReadOnlySpan<uint> ComboScoreValues => [10, 100, 200, 500, 1000, 10000];
    public static void IncreaseComboScore(int comboCounter = 0)
    {
	    ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
    }
}
