using System.Collections.Generic;
using Godot;
using Godot.Collections;
using OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class OptionStorage : VBoxContainer
{
    private Option[] _options;
    
    protected uint Index { get; private set; }
    
    private uint _length;

    public override void _EnterTree() => _options = GetOptions();
    
    public Option Previous => SelectNewOption(Index - 1);
    public Option Next => SelectNewOption(Index + 1);
    public Option First => SelectNewOption(0);
    
    public Option Current
    {
        get
        {
            Option option = _options[Index];
            option.IsSelected = true;
            return option;
        }
    }
    
    private Option[] GetOptions()
    {
        Array<Node> children = GetChildren();
        var options = new List<Option>(children.Count);
        
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is Option option)
            {
                options.Add(option);
            }
        }
        
        _length = (uint)options.Count;
        return options.ToArray();
    }

    private Option SelectNewOption(uint position)
    {
        _options[Index].IsSelected = false;
        Option option = _options[Index = position % _length];
        option.IsSelected = true;
        return option;
    }
}
