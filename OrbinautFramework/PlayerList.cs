using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3;

public readonly struct PlayerList()
{
    public ReadOnlySpan<IPlayer> Values => CollectionsMarshal.AsSpan(_players);
    public int Count => _players.Count;
    
    private readonly List<IPlayer> _players = new(4);
    
    public void Add(IPlayer logic)
    {
        logic.Data.Id = _players.Count;
        _players.Add(logic);
    }
    
    public void Remove(IPlayer logic)
    {
        _players.Remove(logic);
        for (int i = logic.Data.Id; i < _players.Count; i++)
        {
            _players[i].Data.Id--;
        }
    }

    public void MovePlayer(int fromIndex, int toIndex)
    {
        if (fromIndex >= _players.Count || toIndex >= _players.Count)
        {
            throw new IndexOutOfRangeException();
        }

        IPlayer firstPlayer = _players[fromIndex];
        IPlayer secondPlayer = _players[toIndex];

        firstPlayer.Data.Id = toIndex;
        secondPlayer.Data.Id = fromIndex;

        _players[toIndex] = firstPlayer;
        _players[fromIndex] = secondPlayer;
    }

    public IPlayer First() => _players[0];
}