#if !DEBUG
using OrbinautFramework3.Framework;
#endif
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class ControlType(IPlayer player)
{
    public bool IsCpu
    {
        get => _cpuLogic != null;
        set 
        {
            if (value)
            {
                _cpuLogic = new CpuLogic(player.Data, player);
                return;
            }
            
            _cpuLogic = null;
#if !DEBUG
            if (!SharedData.IsDebugModeEnabled) return;
#endif
            _debugMode = new DebugMode(player);
        }
    }
    
    private CpuLogic _cpuLogic;
    private DebugMode _debugMode;

    public bool SwitchDebugMode() => _debugMode != null && _debugMode.Switch();
    public void UpdateDebugMode() => _debugMode?.Update();
    public void UpdateCpu() => _cpuLogic?.Process();
}
