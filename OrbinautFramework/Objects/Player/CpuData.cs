namespace OrbinautFramework3.Objects.Player;

public class CpuData
{
    public CpuStates State { get; set; } = CpuStates.Main;
    public float RespawnTimer { get; set; }
    public float InputTimer { get; set; }
    public bool IsJumping { get; set; }
    public bool IsRespawn { get; set; }
    public ICpuTarget Target { get; set; }
}