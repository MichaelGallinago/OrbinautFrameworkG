using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Data;

public class DeathData
{
    public float RestartTimer { get; set; }
    public Death.States State { get; set; }

    public void Init()
    {
        State = Death.States.Wait;
        IsDead = false;
        RestartTimer = 0f;
    }
}
