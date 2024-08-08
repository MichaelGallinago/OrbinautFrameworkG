namespace OrbinautFramework3.Objects.Player;

public class SuperData
{
    public float Timer { get; set; }
    public bool IsSuper => Timer > 0f;

    public void Init() => Timer = 0f;
}