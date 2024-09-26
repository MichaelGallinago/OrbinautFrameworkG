using System;
using Godot;
using OrbinautFrameworkG.Objects.Common.GiantRing;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Framework.StaticStorages;

public static class SharedData
{
    public static bool ShowSplash { get; set; } = false;
    public static bool SkipBranding { get; set; } = true;
    public static byte? CurrentSaveSlot { get; set; } = null; // null = no-save slot by default
    
    // Common global variables
    public static bool IsDebugModeEnabled { get; set; }
	public static CheckpointData CheckpointData { get; set; }
    public static GiantRingData GiantRingData { get; set; }
    // TODO: ds_giant_rings
    //public static ds_giant_rings { get; set; } = ds_list_create();
    public static Vector2I LifeRewards { get; set; }
    public static int RealPlayerCount { get; set; } = 1;
    public static uint PlayerRings { get; set; }
    public static ShieldContainer.Types[] SavedShields { get; set; }
    
    
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
}
