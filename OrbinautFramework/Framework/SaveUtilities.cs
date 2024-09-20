using System;
using System.IO;
using Godot;
using OrbinautFramework3.Framework.StaticStorages;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

public static class SaveUtilities
{
    private const string SavesPath = "user://save1.bin";
    
    public static byte Slot { get; set; } = 0;
    
    public static void Save()
    {
        string binaryPath = ProjectSettings.GlobalizePath(ConfigPath);
    }
    
    public static void Load()
    {
	    SharedData.EmeraldCount
        string binaryPath = ProjectSettings.GlobalizePath(ConfigPath);
        Span<byte> span = File.ReadAllBytes(binaryPath);
        GD.BytesToVar(span.Slice());
        //GD.VarToBytesWithObjects();
    }

    private partial class DataFile(
	    PlayerNode.Types[] playerTypes, byte emeraldCount, ushort lifeCount,uint scoreCount) : Resource
    {
	    public readonly PlayerNode.Types[] PlayerTypes = playerTypes;
	    public readonly byte EmeraldCount = emeraldCount;
	    public readonly ushort LifeCount = lifeCount;
	    public readonly uint ScoreCount = scoreCount;
    }
    /*
    file_bin_write_byte(_file, global.stage_index);
	file_bin_write_byte(_file, global.player_main);
	file_bin_write_byte(_file, global.player_cpu);
	file_bin_write_byte(_file, global.emerald_count);
	file_bin_write_byte(_file, global.continue_count);
	file_bin_write_byte(_file, global.life_count);
	file_bin_write_byte(_file, global.score_count % 100);
	file_bin_write_byte(_file, floor(global.score_count / 100) % 100);
	file_bin_write_byte(_file, floor(global.score_count / 10000) % 100);
	file_bin_write_byte(_file, floor(global.score_count / 1000000) % 100);
     */
}
