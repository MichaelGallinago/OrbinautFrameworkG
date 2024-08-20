#if !DEBUG
using OrbinautFramework3.Framework;
#endif
using OrbinautFramework3.Objects.Player.Data;

namespace OrbinautFramework3.Objects.Player.Logic;

public class ControlType(IPlayer player)
{
    public bool IsDebugMode => _debugMode is { IsEnabled: true };
    
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

    public bool Process()
    {
        if (!IsCpu) return _debugMode != null && _debugMode.Update();
        
        _cpuLogic.Process();
        return false;
    }
}
