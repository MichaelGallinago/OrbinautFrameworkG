namespace OrbinautFrameworkG.Objects.Player.Data;

public class DamageData
{
    public bool IsInvincible { get; set; }
    public float InvincibilityTimer { get; set; }

    public void Init()
    {
        IsInvincible = false;
        InvincibilityTimer = 0f;
    }
}
