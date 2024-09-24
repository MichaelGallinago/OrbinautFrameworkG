using OrbinautFrameworkG.Objects.Player.Characters.Logic.Base;

namespace OrbinautFrameworkG.Objects.Player.Characters.Logic;

public abstract class CharacterDependencyGenerator
{
    public CpuLogic Cpu { get; protected init; }
    public FlightLogic Flight { get; protected init; }
}
