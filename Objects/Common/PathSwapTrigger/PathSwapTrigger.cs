using Godot;
using OrbinautFramework3.Framework.CommonObject;

namespace OrbinautFramework3.Objects.Common.PathSwapTrigger;

public partial class PathSwapTrigger : CommonObject
{
    [Export] private Sprite2D _sprite;
    
    public PathSwapTrigger()
    {
        Visible	= false;
        //TODO: depth
        //depth = 50;
    }
    
    public override void _Ready()
    {
        if (_sprite != null)
        {
            _sprite.Modulate = new Color(_sprite.Modulate, 0.5f);
        }

        //radius_y = sprite_height / 2;
    }
}