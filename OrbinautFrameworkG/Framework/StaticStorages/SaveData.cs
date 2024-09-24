using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using OrbinautFrameworkG.Objects.Player;

namespace OrbinautFrameworkG.Framework;

public static class SaveData
{
	private const byte SlotLimit = 4;
    
    public static string ScenePath { get; set; }
    public static byte ContinueCount { get; set; } = 3;
    public static byte EmeraldCount { get; set; } = 7;
    public static uint ScoreCount { get; set; }
    public static ushort LifeCount { get; set; } = 3;
    public static List<PlayerNode.Types> PlayerTypes { get; set; } = [PlayerNode.Types.Sonic]; //[PlayerNode.Types.Sonic, PlayerNode.Types.Tails]; TODO: menu and CPU
    
    public static byte Slot
    {
	    get => _slot;
	    set => _currentBinaryPath = (_slot = value) == 0 ? null : GetBinaryPath(value);
    }
    private static byte _slot;
    private static string _currentBinaryPath;
    
    private static readonly uint[] ComboScoreValues = [10, 100, 200, 500, 1000, 10000];
    private static readonly string[] SavesPaths;
    
    static SaveData()
    {
	    SavesPaths = new string[SlotLimit];
	    for (var i = 0; i < SlotLimit; i++)
	    {
		    SavesPaths[i] = $"user://save{i}.bin";
	    }
    }
    
    public static bool IsSaveExists(byte slot) => File.Exists(GetBinaryPath(slot));
     
    public static void Save()
    {
	    if (Slot == 0) return;
	    
	    byte[] playerTypes = PlayerTypes.Select(type => (byte)type).ToArray();
        File.WriteAllBytes(_currentBinaryPath, GD.VarToBytesWithObjects(new DataFile(
	        ScenePath, EmeraldCount, ContinueCount, LifeCount, ScoreCount, playerTypes)));
    }
    
    public static void Load()
    {
	    if (Slot == 0 || !File.Exists(_currentBinaryPath)) return;
	    
	    (ScenePath, EmeraldCount, ContinueCount, LifeCount, ScoreCount, PlayerTypes) 
		    = GD.BytesToVarWithObjects(File.ReadAllBytes(_currentBinaryPath)).As<DataFile>();
    }
    
    public static void IncreaseComboScore(int comboCounter = 0)
    {
	    ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
    }
    
    private static string GetBinaryPath(byte slot) => ProjectSettings.GlobalizePath(SavesPaths[slot - 1]);
}

public partial class DataFile(
	string path, byte emeraldCount, byte continueCount, ushort lifeCount, uint scoreCount, byte[] types) 
	: Resource
{
	public void Deconstruct(out string scenePath, out byte emeralds, out byte continues, 
		out ushort life, out uint score, out List<PlayerNode.Types> playerTypes)
	{
		scenePath = path;
		emeralds = emeraldCount;
		continues = continueCount;
		life = lifeCount;
		score = scoreCount;
		playerTypes = types.Select(b => (PlayerNode.Types)b).ToList();
	}
}
