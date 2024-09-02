using System.Collections.Generic;
using Godot;
using Godot.Collections;
using OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class OptionStorage : VBoxContainer
{
    public Option[] Options { get; private set; }

    public override void _Ready()
    {
        Options = GetOptions();
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

        return options.ToArray();
    }
}