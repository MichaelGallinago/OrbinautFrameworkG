using System;

namespace OrbinautFramework3.Scenes.Screens.DevMenu.Prefabs.Settings;

public partial class MultiplierSetting : Setting
{
    public byte Value
    {
        set
        {
#if TOOLS
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Value));
            }
#endif
            
            ValueText = value.ToString();
        }
    }
}
