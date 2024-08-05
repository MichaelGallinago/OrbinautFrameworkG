using Godot;

namespace OrbinautFramework3.Framework.ObjectBase;

public abstract partial class OrbinautNode : Node2D, ICullable
{
    [Export] public HitBox HitBox { get; init; }
    [Export] public SolidBox SolidBox { get; init; }
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