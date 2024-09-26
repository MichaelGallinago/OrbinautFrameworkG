using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class WaterData
{
    public bool IsUnderwater { get; set; }
    public float AirTimer { get; set; }

    public void Init()
    {
        IsUnderwater = false;
        AirTimer = Constants.DefaultAirTimer;
    }
}