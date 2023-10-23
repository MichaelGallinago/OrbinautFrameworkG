namespace OrbinautFramework3.Objects.Spawnable.Barrier;

public partial class Barrier : Framework.CommonObject.CommonObject
{
    public float Angle { get; set; }
    public Player.Player Target { get; set; }

    public Barrier(Player.Player target)
    {
        Target = target;
    }
    
    public override void _Ready()
    {
        base._Ready();
    }
}