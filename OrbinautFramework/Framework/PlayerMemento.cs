using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework;

public class PlayerMemento(PlayerData originator) : IMemento
{
    public Vector2 Position { get; } = originator.PlayerNode.Position;
    private readonly Vector2 _scale = originator.PlayerNode.Scale;
    private readonly int _zIndex = originator.PlayerNode.ZIndex;
    
    public void Reset()
    {
        originator.PlayerNode.Position = Position;
        originator.PlayerNode.ZIndex = _zIndex;
        originator.PlayerNode.Scale = _scale;

        originator.Init();
    }
}
