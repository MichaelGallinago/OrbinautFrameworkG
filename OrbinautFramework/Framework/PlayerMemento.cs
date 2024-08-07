using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework;

public class PlayerMemento(PlayerData originator) : IMemento
{
    public Vector2 Position { get; } = originator.Player.Position;
    private readonly Vector2 _scale = originator.Player.Scale;
    private readonly int _zIndex = originator.Player.ZIndex;
    
    public void Reset()
    {
        originator.Player.Position = Position;
        originator.Player.ZIndex = _zIndex;
        originator.Player.Scale = _scale;

        originator.Init();
    }
}
