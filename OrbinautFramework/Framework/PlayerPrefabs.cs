using Godot;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

[GlobalClass]
public partial class PlayerPrefabs : Resource
{
    [Export] private PackedScene _sonic;
    [Export] private PackedScene _tails;
    [Export] private PackedScene _knuckles;
    [Export] private PackedScene _amy;
    
    public PackedScene Get(PlayerNode.Types type) => type switch
    {
        PlayerNode.Types.Tails => _tails,
        PlayerNode.Types.Knuckles => _knuckles,
        PlayerNode.Types.Amy => _amy,
        _ => _sonic
    };
}
