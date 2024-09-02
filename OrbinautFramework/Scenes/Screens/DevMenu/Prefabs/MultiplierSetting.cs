using System;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class MultiplierSetting : Setting
{
    public byte Value
    {
        set
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Value));
            }
            
            ValueText = value.ToString();
        }
    }
}
