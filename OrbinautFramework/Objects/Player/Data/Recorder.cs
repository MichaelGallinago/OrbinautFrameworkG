using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Modules;

namespace OrbinautFramework3.Objects.Player.Data;

public class Recorder
{
    private const byte MinimalRecordLength = 32;
    
    public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
    private DataRecord[] _recordedData;
    
    public static void ResizeAll()
    {
        if (Scene.Instance.Time == 0f) return;

        ReadOnlySpan<Player> players = Scene.Instance.Players.Values;
        int playersCount = players.Length + 1;
        foreach (Player player in players)
        {
            Resize(playersCount);
        }
    }
    
    public void Record()
    {
        Array.Copy(_recordedData, 0, _recordedData, 
            1, _recordedData.Length - 1);
		
        _recordedData[0] = new DataRecord(Owner.Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
    }
    
    public void Fill()
    {
        _recordedData = new DataRecord[Math.Max(MinimalRecordLength, CpuModule.DelayStep * Scene.Instance.Players.Count)];
        var record = new DataRecord(Owner.Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
        
        Array.Fill(_recordedData, record);
    }
    
    private void Resize(int playersCount)
    {
        int newLength = Math.Max(MinimalRecordLength, CpuModule.DelayStep * playersCount);
        int oldLength = _recordedData.Length;
		
        if (newLength <= oldLength)
        {
            Array.Resize(ref _recordedData, newLength);
            return;
        }
		
        var resizedData = new DataRecord[newLength];
        var record = new DataRecord(Owner.Position, Input.Press, Input.Down, Facing, SetPushAnimationBy);
		
        Array.Copy(_recordedData, resizedData, oldLength);
        Array.Fill(resizedData, record,oldLength, newLength - oldLength);
        _recordedData = resizedData;
    }
}