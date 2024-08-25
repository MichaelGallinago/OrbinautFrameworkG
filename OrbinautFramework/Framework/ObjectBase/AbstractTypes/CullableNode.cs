using Godot;

namespace OrbinautFramework3.Framework.ObjectBase.AbstractTypes;

public partial class CullableNode : Node2D, ICullable, IPosition
{
    [Export] public ICullable.Types CullingType
    {
        get => _culling;
        set
        {
            switch (_culling)
            {
                case ICullable.Types.None:
                    ObjectCuller.Local.AddToCulling(this);
                    break;
                default:
                    if (value != ICullable.Types.None) break;
                    ObjectCuller.Local.RemoveFromCulling(this);
                    break;
            }

            _culling = value;
        }
    }
    private ICullable.Types _culling;
    
    public new Vector2 Position
    {
        get => _floatPosition;
        set
        {
            base.Position = (Vector2I)value;
            _floatPosition = value;
        }
    }
    private Vector2 _floatPosition;
    
    public override void _EnterTree()
    {
        if (CullingType != ICullable.Types.None)
        {
            ObjectCuller.Local.AddToCulling(this);
        }
    }
    
    public override void _ExitTree()
    {
        if (CullingType != ICullable.Types.None)
        {
            ObjectCuller.Local.RemoveFromCulling(this);
        }
    }
}
