using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

public class TailsLogic : PlayerLogic
{
    private readonly Carry _carry;
    public CarryData CarryData { get; } = new();
    
    public TailsLogic(IPlayerNode playerNode, IPlayerSprite sprite) : base(playerNode, sprite)
    {
        _carry = new Carry(Data, CarryData, this);
        DataUtilities.DataReset += CarryData.Free;
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
