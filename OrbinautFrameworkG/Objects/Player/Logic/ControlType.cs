#if !DEBUG
using OrbinautFrameworkG.Framework.StaticStorages;
#endif
using OrbinautFrameworkG.Objects.Player.Data;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public class ControlType(IPlayer player, Characters.Logic.Base.CpuLogic cpuLogic)
{
    public bool IsCpu
    {
        get => _cpuLogic != null;
        set 
        {
            if (value)
            {
                _cpuLogic = new CpuLogic(player.Data, player, cpuLogic);
                return;
            }
            
            _cpuLogic = null;
#if !DEBUG
            if (!EditMode.IsAllowed) return;
#endif
            _editMode = new EditMode(player);
        }
    }
    
    private CpuLogic _cpuLogic;
    private EditMode _editMode;
    
    public void UpdateCpu() => _cpuLogic?.Process();
    public bool SwitchDebugMode() => _editMode != null && _editMode.Switch();
    public void UpdateDebugMode()
    {
        _editMode?.Update();
        player.Data.Movement.Position = player.Position;
    }
}
