using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;
using OrbinautFrameworkG.Objects.Player.Sprite;

namespace OrbinautFrameworkG.Objects.Player.Characters.Tails;

public class TailsLogic : PlayerLogic
{
    private readonly Carry _carry;
    public CarryData CarryData { get; } = new();
    
    public TailsLogic(IPlayerNode playerNode, IPlayerSprite sprite) : base(playerNode, sprite)
    {
        _carry = new Carry(Data, CarryData, this);
        DataUtilities.DataReseted += CarryData.Free;
        Landing.Landed += () => CarryData.Target?.OnFree();
    }
    
    protected override void ProcessLateControl()
    {
        _carry.Process();
        base.ProcessLateControl();
    }
    
    public override void Init()
    {
        base.Init();
        CarryData.Init();
    }
}
