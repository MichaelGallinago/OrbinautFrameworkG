using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerNode : IPosition
{
    //TODO: limit set access in player
    int ZIndex { get; set; }
    bool Visible { get; set; }
    Vector2 Scale { get; set; }
    float RotationDegrees { get; set; }

    void SetData(int zIndex, bool visible, Vector2 scale, Vector2 position)
    {
        Scale = scale;
        ZIndex = zIndex;
        Visible = visible;
        Position = position;
    }
    
    HitBox HitBox { get; }
    SolidBox SolidBox { get; }
    PlayerNode.Types Type { get; }
    ShieldContainer Shield { get; }
    Vector2 PreviousPosition { get; }
}
