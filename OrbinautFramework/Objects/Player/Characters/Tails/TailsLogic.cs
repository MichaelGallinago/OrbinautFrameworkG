using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Characters.Tails;

public class TailsLogic : PlayerLogic, ICarrier
{
    private readonly Carry _carry;
    
    public TailsLogic(IPlayerNode playerNode, IPlayerSprite sprite) : base(playerNode, sprite)
    {
        _carry = new Carry(Data, this, this);
    }

    protected override void ProcessLateControl()
    {
        _carry.Process();
        base.ProcessLateControl();
    }
}
