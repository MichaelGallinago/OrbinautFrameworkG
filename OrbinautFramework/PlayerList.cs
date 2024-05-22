using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3;

public readonly struct PlayerList
{
    public ReadOnlySpan<Player> Values => CollectionsMarshal.AsSpan(_players);
    public int Count => _players.Count;
    
    private readonly List<Player> _players = [];

    public PlayerList() {}

    public void Add(Player data)
    {
        data.Id = _players.Count;
        _players.Add(data);
    }
    
    public void Remove(Player data)
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

        Player firstPlayer = _players[fromIndex];
        Player secondPlayer = _players[toIndex];

        firstPlayer.Id = toIndex;
        secondPlayer.Id = fromIndex;

        _players[toIndex] = firstPlayer;
        _players[fromIndex] = secondPlayer;
    }

    public Player First() => _players[0];
}