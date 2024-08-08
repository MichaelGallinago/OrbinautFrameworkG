using Godot;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Data;

public interface IPlayerNode
{
    Vector2 Position { get; set; }
    Vector2 Scale { get; set; }
    int ZIndex { get; set; }
    float RotationDegrees { get; set; }
    float Rotation { get; set; }
    bool Visible { get; set; }
    
    PlayerNode.Types Type { get; }
    HitBox HitBox { get; }
    SolidBox SolidBox { get; }
    ShieldContainer Shield { get; }
    PlayerAnimatedSprite Sprite { get; }
    
    void Init();
    bool IsInstanceValid();
}
