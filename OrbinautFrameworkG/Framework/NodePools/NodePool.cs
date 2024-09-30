using Godot;
using Godot.Collections;

namespace OrbinautFrameworkG.Framework.NodePools;

[Tool]
public abstract partial class NodePool<[MustBeVariant]T1, T2> : Node where T1 : Node where T2 : NodePool<T1, T2>
{
    public static T2 Instance { get; private set; }

    [Export] private PackedScene _node;
    
    [Export]
    private Array<T1> PooledNodes
    {
        get => _pooledNodes;
        set => _ = value;
    }
    
    [Export]
    private int DefaultPoolSize
    {
        get => _defaultPoolSize;
        set
        {
            ChangeDefaultPoolSize(value);
            _defaultPoolSize = value;
        }
    }

    private Array<T1> _pooledNodes = [];

    private int _defaultPoolSize = 4;

    private int _lastIndex;

    protected NodePool()
    {
        SetInstance();
    }
    
    private void SetInstance()
    {
        if (Instance == null)
        {
            Instance = (T2)this;
            return;
        }
        
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (Instance != this) return;
        Instance = null;
    }
    
    public T1 Get()
    {
        if (_lastIndex < _pooledNodes.Count)
            return _pooledNodes[_lastIndex++];

        var newNode = _node.Instantiate<T1>();
        _pooledNodes.Add(newNode);
        _lastIndex++;
        return newNode;
    }

    public void Return(T1 node)
    {
        if (_lastIndex > 0)
            _pooledNodes[--_lastIndex] = node;
        
        _pooledNodes.Add(node);
    }

    private void ChangeDefaultPoolSize(int newSize)
    {
        if (newSize > _defaultPoolSize)
        {
            for (var i = 0; i < newSize - _defaultPoolSize; i++)
            {
                var newNode = _node.Instantiate<T1>();
                newNode.SetOwner(this);
                AddChild(newNode);
                _pooledNodes.Add(newNode);
            }
        }
        else if (newSize < _defaultPoolSize)
        {
            for (var i = 1; i <= _defaultPoolSize - newSize; i++)
            {
                int index = _defaultPoolSize - i;
                _pooledNodes[index].QueueFree();
                _pooledNodes.RemoveAt(index);
            }
        }
    }
}