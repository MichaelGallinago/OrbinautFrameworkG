using System;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs;

public partial class PercentSetting : Setting
{
    public byte Value 
    { 
        set
        {
            if (value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(Value));
            }
            
            ValueText = value.ToString();
        }
    }
}
