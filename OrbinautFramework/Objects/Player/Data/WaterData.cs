using OrbinautFramework3.Framework;

namespace OrbinautFramework3.Objects.Player.Data;

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