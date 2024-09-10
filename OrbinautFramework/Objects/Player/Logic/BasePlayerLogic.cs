using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public class BasePlayerLogic : PlayerLogic
{
    public BasePlayerLogic(IPlayerNode playerNode, IPlayerSprite sprite) : base(playerNode, sprite)
    {
        CharacterCpuLogic cpuLogic = new CarrierCpuLogic();
    }
}