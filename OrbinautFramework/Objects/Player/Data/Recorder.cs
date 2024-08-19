using System;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Data;

public class Recorder(PlayerData data, IPlayerLogic logic)
{
    private const byte MinimalLength = 32;
    
    public ReadOnlySpan<DataRecord> RecordedData => _recordedData;
    
    private DataRecord NewRecord => new(
        data.Node.Position, 
        data.Input.Press, data.Input.Down, 
        data.Visual.Facing, data.Visual.SetPushBy, 
        data.Movement.IsJumping, data.Movement.IsGrounded, 
        logic.Action);
    
    private DataRecord[] _recordedData;
    
    public static void ResizeAll()
    {
        if (Scene.Instance.Time == 0f) return;

        ReadOnlySpan<IPlayer> players = Scene.Instance.Players.Values;
        int playersCount = players.Length + 1;
        foreach (IPlayer player in players)
        {
            player.Recorder.Resize(playersCount);
        }
    }
    
    public void Record()
    {
        int length = _recordedData.Length - 1;
        Array.Copy(_recordedData, 0, _recordedData, 1, length);
		
        _recordedData[0] = NewRecord;
    }
    
    public void Fill()
    {
        _recordedData = new DataRecord[Math.Max(MinimalLength, CpuLogic.DelayStep * Scene.Instance.Players.Count)];
        Array.Fill(_recordedData, NewRecord);
    }
    
    private void Resize(int playersCount)
    {
        int newLength = Math.Max(MinimalLength, CpuLogic.DelayStep * playersCount);
        int oldLength = _recordedData.Length;
		
        if (newLength <= oldLength)
        {
            Array.Resize(ref _recordedData, newLength);
            return;
        }
        
        var resizedData = new DataRecord[newLength];
		
        Array.Copy(_recordedData, resizedData, oldLength);
        Array.Fill(resizedData, NewRecord,oldLength, newLength - oldLength);
        _recordedData = resizedData;
    }
}
