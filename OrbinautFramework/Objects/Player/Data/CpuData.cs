using OrbinautFramework3.Objects.Player.Modules;

namespace OrbinautFramework3.Objects.Player.Data;

public class CpuData
{
    public bool IsJumping { get; set; }
    public float InputTimer { get; set; }
    public ICpuTarget Target { get; set; }
    public float RespawnTimer { get; set; }
    public CpuModule.States State { get; set; }
    
    public void Init()
    {
        State = CpuModule.States.Main;
        Target = null;
        IsJumping = false;
        InputTimer = 0f;
        RespawnTimer = 0f;
    }
}
