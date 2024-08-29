using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Data;

public class CpuData
{
    public bool IsJumping { get; set; }
    public float InputTimer { get; set; }
    public IPlayer Target { get; set; }
    public float RespawnTimer { get; set; }
    public CpuLogic.States State { get; set; }
    
    public void Init()
    {
        State = CpuLogic.States.Main;
        Target = null;
        IsJumping = false;
        InputTimer = 0f;
        RespawnTimer = 0f;
    }
}
