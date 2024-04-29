using Godot;
using OrbinautFramework3.Audio.Player;

namespace OrbinautFramework3.Framework;

public abstract partial class Stage : Scene
{
    public string ZoneName { get; set; } = "UNKNOWN";
    public byte ActId { get; set; }
    public int InitialWaterLevel { get; } = ushort.MaxValue;
    public int WaterLevel { get; set; } = ushort.MaxValue;
    public bool IsWaterEnabled { get; set; }
    public AudioStream Music { get; set; }
    
    /*
    animal_set       =  [];
    next_stage	     =  noone;
    save_progress    =  false;
    */
    
    public Stage()
    {
        IsStage = true;
    }

    public override void _Ready()
    {
        //TODO: CommonStage init
        base._Ready();
        if (Music != null)
        {
            AudioPlayer.Music.Play(Music);
        }
    }
}
