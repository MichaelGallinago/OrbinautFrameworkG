using Godot;
using Godot.Collections;

namespace OrbinautFrameworkG.Framework.NodePools;

[Tool]
public abstract partial class NodePool<[MustBeVariant]T1, T2> : Node where T1 : Node where T2 : NodePool<T1, T2>
{
    [Export] private PackedScene _node;

    [Export]
    private Array<T1> PooledNodes
    {
        get => _pooledNodes;
        set => _ = value;
    }

    private Array<T1> _pooledNodes = [ null ];
    
    public static T2 Instance { get; private set; }
    
    public T1 GetInstance()
    {
        for (int i = 0; i < _pooledNodes.Count; i++)
        {
        }
        return _node.Instantiate<T1>();
    }
}