using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3;

public readonly struct PlayerList()
{
    public ReadOnlySpan<PlayerData> Values => CollectionsMarshal.AsSpan(_players);
    public int Count => _players.Count;
    
    private readonly List<PlayerData> _players = [];
    
    public void Add(PlayerData data)
    {
        data.Id = _players.Count;
        _players.Add(data);
    }
    
    public void Remove(PlayerData data)
    {
        _players.Remove(data);
        for (int i = data.Id; i < _players.Count; i++)
        {
            _players[i].Id--;
        }
    }

    public void MovePlayer(int fromIndex, int toIndex)
    {
        if (fromIndex >= _players.Count || toIndex >= _players.Count)
        {
            throw new IndexOutOfRangeException();
        }

        PlayerData firstPlayer = _players[fromIndex];
        PlayerData secondPlayer = _players[toIndex];

        firstPlayer.Id = toIndex;
        secondPlayer.Id = fromIndex;

        _players[toIndex] = firstPlayer;
        _players[fromIndex] = secondPlayer;
    }

    public PlayerData First() => _players[0];
}