namespace OrbinautFramework3.Objects.Player.Data;

public class ItemData
{
    public uint ComboCounter { get; set; }
    public float SpeedTimer { get; set; }
    public float InvincibilityTimer { get; set; }

    public void Init()
    {
        SpeedTimer = 0f;
        ComboCounter = 0;
        InvincibilityTimer = 0f;
    }
}
