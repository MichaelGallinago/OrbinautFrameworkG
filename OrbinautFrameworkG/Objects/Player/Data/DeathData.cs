using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class DeathData
{
    public float RestartTimer { get; set; }
    public Death.States State { get; set; }

    public void Init()
    {
        State = Death.States.Wait;
        RestartTimer = 0f;
    }
}
