using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player;
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Framework;

public class PlayerMemento(PlayerNode originator) : IMemento
{
    public Vector2 Position { get; } = originator.Position;
    private readonly Vector2 _scale = originator.Scale;
    private readonly int _zIndex = originator.ZIndex;
    
    public void Reset()
    {
        originator.Position = Position;
        originator.ZIndex = _zIndex;
        originator.Scale = _scale;

        originator.Init();
    }
}
