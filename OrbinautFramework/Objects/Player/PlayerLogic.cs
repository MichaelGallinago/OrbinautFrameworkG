using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;
using OrbinautFramework3.Objects.Player.PlayerActions;

namespace OrbinautFramework3.Objects.Player;

public class PlayerLogic : IStateHolder<ActionFsm.States>
{
    public CarryTarget CarryTarget { get; } = new();
    
    private readonly Recorder _recorder;
    private readonly DebugMode _debugMode;
    private readonly CpuModule _cpuModule;
    private readonly PlayerData _data;
    
    public ActionFsm.States State
    {
        get => _actionFsm.State;
        set => _actionFsm.State = value;
    }
    
    private Carry _carry;
    private Death _death;
    private Water _water;
    private Damage _damage;
    private Status _status;
    private Palette _palette;
    private ActionFsm _actionFsm;
    private PhysicsCore _physicsCore;
    private AngleRotation _angleRotation;
    private CollisionBoxes _collisionBoxes;
    private Initialization _initialization;
    private ObjectInteraction _objectInteraction;

    public PlayerLogic(IPlayerNode playerNode)
    {
        _data = new PlayerData(this, playerNode);

        _recorder = new Recorder(_data);
        
        _carry = new Carry(_data);
        _death = new Death(_data);
        _water = new Water(_data);
        _damage = new Damage(_data);
        _status = new Status(_data);
        _palette = new Palette(_data);
        _actionFsm = new ActionFsm(_data);
        _physicsCore = new PhysicsCore(_data);
        _angleRotation = new AngleRotation(_data);
        _collisionBoxes = new CollisionBoxes(_data);
        _initialization = new Initialization(_data);
        _objectInteraction = new ObjectInteraction(_data);
        
        _cpuModule = new CpuModule(_data);
        LandHandler += () => _actionFsm.OnLand();
        
        Recorder.ResizeAll();
        Scene.Instance.Players.Add(_data);
        
        if (_data.Id == 0)
        {
#if !DEBUG
            if (!SharedData.IsDebugModeEnabled) return;
#endif
            _debugMode = new DebugMode(_data);
        }
    }

    public void Init()
    {
        _initialization.Init();
        _initialization.Spawn();
        _cpuModule?.Init();
        _recorder.Fill();
    }

    public void ExitTree()
    {
        Scene.Instance.Players.Remove(_data);
        Recorder.ResizeAll();
    }

    public void Process()
    {
        _data.Input.Update(_data.Id);
        
        if (_debugMode != null && _data.Death.State == Death.States.Wait)
        {
            if (_debugMode.Update(_data.Input)) return;
        }
        
        _cpuModule?.Process();
        _death.Process();
		
        if (_data.Movement.IsControlRoutineEnabled)
        {
            RunControlRoutine();
        }
        
        if (!_data.Death.IsDead)
        {
            _water.Process();
            _status.Update();
            _collisionBoxes.Update();
        }
		
        _recorder.Record();
        _angleRotation.Process();
        _palette.Process();
    }
    
    private void RunControlRoutine()
    {
        _data.Physics.Update(_data.Water.IsUnderwater, _data.Super.IsSuper, _data.Node.Type, _data.Item.SpeedTimer);
        
        _actionFsm.EarlyPerform();
        _actionFsm.Perform();
        
        if (_data.Movement.IsCorePhysicsSkipped)
        {
            _data.Movement.IsCorePhysicsSkipped = false;
            return;
        }
        
        _physicsCore.ProcessCorePhysics();
        
        _actionFsm.LatePerform();
        _carry.Process();
    }
}
