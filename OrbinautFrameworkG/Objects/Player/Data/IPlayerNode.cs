using Godot;
using OrbinautFrameworkG.Framework.ObjectBase;
using OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Objects.Player.Data;

public interface IPlayerNode : ISolid, IInteractive
{
    //TODO: limit set access in player
    int ZIndex { get; set; }
    bool Visible { get; set; }
    new Vector2 Scale { get; set; }
    float RotationDegrees { get; set; }

    void SetData(int zIndex, bool visible, Vector2 scale, Vector2 position)
    {
        Scale = scale;
        ZIndex = zIndex;
        Visible = visible;
        Position = position;
    }
    
    PlayerNode.Types Type { get; }
    ShieldContainer Shield { get; }
}
