using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;

public class BaseDependencyGenerator : CharacterDependencyGenerator
{
    public BaseDependencyGenerator(IPlayerLogic logic, PlayerData data)
    {
        Cpu = new CpuLogic(logic);
        Flight = new FlightLogic(data);
    }
}
