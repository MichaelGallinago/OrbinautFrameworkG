using System.Collections.Generic;
using Godot;
using Godot.Collections;
using OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class OptionStorage : VBoxContainer
{
    private Option[] _options;
    private uint _length;
    private uint _index;
    
    public override void _Ready() => _options = FilterNodes<Option>();
    
    public Option Previous => SelectNewOption(_index - 1);
    public Option Next => SelectNewOption(_index + 1);
    public Option First => SelectNewOption(0);
    
    public Option Current
    {
        get
        {
            Option option = _options[_index];
            option.IsSelected = true;
            return option;
        }
    }
    
    protected T[] FilterNodes<T>() where T : Node
    {
        Array<Node> children = GetChildren();
        var nodes = new List<T>(children.Count);
        
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is T node)
            {
                nodes.Add(node);
            }
        }
        
        _length = (uint)nodes.Count;
        return nodes.ToArray();
    }

    private Option SelectNewOption(uint position)
    {
        _options[_index].IsSelected = false;
        Option option = _options[_index = position % _length];
        option.IsSelected = true;
        return option;
    }
}
