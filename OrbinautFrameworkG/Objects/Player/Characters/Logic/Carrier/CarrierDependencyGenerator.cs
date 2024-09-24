using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Logic;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic.Carrier;

public class CarrierDependencyGenerator : CharacterDependencyGenerator
{
    public CarrierDependencyGenerator(IPlayerLogic logic, PlayerData data, CarryData carryData)
    {
        Cpu = new CpuLogic(logic, carryData);
        Flight = new FlightLogic(data, carryData);
    }
}