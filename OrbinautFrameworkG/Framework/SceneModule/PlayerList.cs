using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Framework.SceneModule;

public class PlayerList
{
    private const byte BasePlayerCapacity = 4;
    
    public ReadOnlySpan<IPlayer> Values => CollectionsMarshal.AsSpan(_players);
    public IMultiTypeEvent<IPlayerCountObserver> CountChanged { get; }
    public int Count => _players.Count;
    
    private readonly MultiTypeDelegate<IPlayerCountObserver, int> _countChanged = new(BasePlayerCapacity);
    private readonly List<IPlayer> _players = new(BasePlayerCapacity);

    public PlayerList() => CountChanged = _countChanged;
    
    public void Add(IPlayer player)
    {
        player.Data.Id = _players.Count;
        _players.Add(player);
        _countChanged.Invoke(_players.Count);
    }
    
    public void Remove(IPlayer player)
    {
        _players.Remove(player);
        for (int i = player.Data.Id; i < _players.Count; i++)
        {
            _players[i].Data.Id--;
        }
        _countChanged.Invoke(_players.Count);
    }

    public void Move(int fromIndex, int toIndex) //TODO: use this
    {
#if TOOLS
        if (fromIndex >= _players.Count || toIndex >= _players.Count)
        {
            throw new IndexOutOfRangeException();
        }
#endif

        IPlayer firstPlayer = _players[fromIndex];
        IPlayer secondPlayer = _players[toIndex];

        firstPlayer.Data.Id = toIndex;
        secondPlayer.Data.Id = fromIndex;

        _players[toIndex] = firstPlayer;
        _players[fromIndex] = secondPlayer;
    }

    public IPlayer FindNearest(Vector2 position)
    {
        if (Count == 1) return _players[0];
        
        var minDistance = float.MaxValue;
        IPlayer nearestPlayer = null;
        
        foreach (IPlayer player in _players)
        {
            float currentDistance = position.DistanceSquaredTo(player.Position);
            if (currentDistance >= minDistance) continue;
            minDistance = currentDistance;
            nearestPlayer = player;
        }
        
        return nearestPlayer;
    }

    public IPlayer FirstOrDefault() => _players.FirstOrDefault();
}
