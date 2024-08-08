using Godot;
using OrbinautFramework3.Objects.Player;

namespace OrbinautFramework3.Framework;

//TODO: fill this
public partial class PrefabStorage : Resource
{
    [Export] public PackedScene PlayerSonic { get; private set; }
    [Export] public PackedScene PlayerTails { get; private set; }
    [Export] public PackedScene PlayerKnuckles { get; private set; }
    [Export] public PackedScene PlayerAmy { get; private set; }
    
    public PackedScene GetPlayer(PlayerNode.Types type) => type switch
    {
        PlayerNode.Types.Tails => PlayerTails,
        PlayerNode.Types.Knuckles => PlayerKnuckles,
        PlayerNode.Types.Amy => PlayerAmy,
        _ => PlayerSonic
    };
}
