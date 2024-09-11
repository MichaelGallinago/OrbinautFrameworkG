using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Logic;

namespace OrbinautFramework3.Objects.Player.Characters.Logic.Base;

public class BaseDependencyGenerator : CharacterDependencyGenerator
{
    public BaseDependencyGenerator(IPlayerLogic logic, PlayerData data)
    {
        Cpu = new CpuLogic(logic);
        Flight = new FlightLogic(data);
    }
}
