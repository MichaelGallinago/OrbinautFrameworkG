using Godot;
using System;

public class StageData
{
    public static CollisionTileMap CollisionTileMap { get; set; }
    public static string ZoneName { get; set; }
    public static byte ActId { get; set; }
    public static byte BgmTrack { get; set; }
    /*
    BgmTrack        =  noone;
    animal_set       =  [];
    water_enabled    = -1;
    water_level_init =  0;
    water_level		 =  0;
    next_stage	     =  noone;
    save_progress    =  false;
    */

    public StageData()
    {
        ZoneName = "UNKNOWN";
        ActId = 0;
    }

    public static void StageSetup()
    {
        
    }
}
