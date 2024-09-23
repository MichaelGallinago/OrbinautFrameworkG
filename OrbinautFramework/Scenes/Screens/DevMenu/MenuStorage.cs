using Godot;
using System.Collections.Generic;
using Godot.Collections;

namespace OrbinautFramework3.Scenes.Screens.DevMenu;

public partial class MenuStorage : Control
{
    [Export] private DevMenu _devMenu;
    
    private Menu[] _menus;
    private uint _length;
    
    public override void _EnterTree()
    {
        _menus = GetMenus();
        
        foreach (Menu menu in _menus)
        {
            menu.Visible = false;
        }
    }


    private Menu[] GetMenus()
    {
        Array<Node> children = GetChildren();
        var menus = new List<Menu>(children.Count);
        
        for (var i = 0; i < children.Count; i++)
        {
            if (children[i] is Menu menu)
            {
                menus.Add(menu);
            }
        }
        
        _length = (uint)menus.Count;
        return menus.ToArray();
    }
}
