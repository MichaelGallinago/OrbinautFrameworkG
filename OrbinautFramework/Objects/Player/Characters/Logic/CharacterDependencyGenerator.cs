using OrbinautFramework3.Objects.Player.Characters.Logic.Base;

namespace OrbinautFramework3.Objects.Player.Characters.Logic;

public abstract class CharacterDependencyGenerator
{
    public CpuLogic Cpu { get; protected init; }
    public FlightLogic Flight { get; protected init; }
}
