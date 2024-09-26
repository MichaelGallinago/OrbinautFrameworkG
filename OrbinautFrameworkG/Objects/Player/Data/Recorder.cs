using System;
using Godot;
using OrbinautFrameworkG.Framework.SceneModule;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class Recorder(PlayerData playerData)
{
    private const byte MinimalLength = 32;
    
    public ReadOnlySpan<DataRecord> Data => _data;
    
    private DataRecord NewRecord => new(
        (Vector2I)playerData.Movement.Position,
        playerData.Input.Press, 
        playerData.Input.Down, 
        playerData.Visual.Facing, 
        playerData.Visual.SetPushBy
    );
    
    private DataRecord[] _data;
    
    public void Record()
    {
        int length = _data.Length - 1;
        Array.Copy(_data, 0, _data, 1, length);
		
        _data[0] = NewRecord;
    }
    
    public void Fill()
    {
        _data = new DataRecord[Math.Max(MinimalLength, CpuLogic.DelayStep * Scene.Instance.Players.Count)];
        Array.Fill(_data, NewRecord);
    }
    
    public void Resize(int playersCount)
    {
        int newLength = Math.Max(MinimalLength, CpuLogic.DelayStep * playersCount);
        int oldLength = _data.Length;
		
        if (newLength <= oldLength)
        {
            Array.Resize(ref _data, newLength);
            return;
        }
        
        var resizedData = new DataRecord[newLength];
		
        Array.Copy(_data, resizedData, oldLength);
        Array.Fill(resizedData, NewRecord,oldLength, newLength - oldLength);
        _data = resizedData;
    }
}
