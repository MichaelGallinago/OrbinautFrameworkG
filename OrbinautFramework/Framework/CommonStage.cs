using Godot;

namespace OrbinautFramework3.Framework;

public abstract partial class CommonStage : CommonScene
{
    public string ZoneName { get; set; }
    public byte ActId { get; set; }
    public int WaterLevel { get; set; }
    public bool IsWaterEnabled { get; set; }
    public int InitialWaterLevel { get; set; }
    public AudioStream Music { get; set; }
    
        /*
    animal_set       =  [];
    water_enabled    = -1;
    water_level_init =  0;
    next_stage	     =  noone;
    save_progress    =  false;
    */
    
    public CommonStage()
    {
        IsStage = true;
        ZoneName = "UNKNOWN";
        ActId = 0;
        WaterLevel = InitialWaterLevel = ushort.MaxValue;
        IsWaterEnabled = false;
    }

    protected abstract void StageSetup();
}