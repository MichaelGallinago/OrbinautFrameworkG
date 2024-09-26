using Godot;
using OrbinautFrameworkG.Framework.Culling;

namespace OrbinautFrameworkG.Framework.ObjectBase.AbstractTypes;

public partial class CullableNode : Node2D, ICullable, IPosition
{
    public ICullable.Types CullingType
    {
        get => _cullingType;
        set
        {
            bool isCullable = value != ICullable.Types.None;
            switch (_cullingType)
            {
                case ICullable.Types.None when isCullable:
                    SceneModule.Scene.Instance.Culler.Add(this);
                    break;
                default:
                    if (isCullable) break;
                    SceneModule.Scene.Instance.Culler.Remove(this);
                    break;
            }
            
            _cullingType = value;
        }
    }
    [Export] private ICullable.Types _cullingType;
    
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
            SceneModule.Scene.Instance.Culler.Add(this);
        }
    }
    
    public override void _ExitTree()
    {
        if (CullingType != ICullable.Types.None)
        {
            SceneModule.Scene.Instance.Culler.Remove(this);
        }
    }
}
