namespace OrbinautFramework3.Objects.Player.Data;

public class DamageData
{
    public bool IsHurt { get; set; }
    public bool IsInvincible { get; set; }
    public float InvincibilityTimer { get; set; }

    public void Init()
    {
        IsHurt = false;
        IsInvincible = false;
        InvincibilityTimer = 0f;
    }
}
