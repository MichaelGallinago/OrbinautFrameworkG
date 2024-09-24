namespace OrbinautFrameworkG.Objects.Player.Data;

public class SuperData
{
    public float Timer { get; set; }
    public bool IsSuper => Timer > 0f;

    public void Init() => Timer = 0f;
}