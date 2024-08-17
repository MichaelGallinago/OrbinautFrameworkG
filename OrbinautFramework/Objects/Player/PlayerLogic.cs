using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.PlayerActions;

namespace OrbinautFramework3.Objects.Player;

public class PlayerLogic : IPlayer
{
    public PlayerData Data { get; }
    public Recorder Recorder { get; }
    public CarryTarget CarryTarget { get; }
    
    public Damage Damage { get; }
    public Landing Landing { get; }
    public DataUtilities DataUtilities { get; }
    public ObjectInteraction ObjectInteraction { get; }
    
    private readonly CpuModule _cpuModule;
    private readonly DebugMode _debugMode;
    
    private Carry _carry;
    private Death _death;
    private Water _water;
    private Status _status;
    private Palette _palette;
    private ActionFsm _actionFsm;
    private PhysicsCore _physicsCore;
    private AngleRotation _angleRotation;
    private CollisionBoxes _collisionBoxes;
    private Initialization _initialization;
    
    public PlayerLogic(IPlayerNode playerNode)
    {
        Data = new PlayerData(playerNode);
        Recorder = new Recorder(Data);
        CarryTarget = new CarryTarget(Data);
        
        Damage = new Damage(Data, this);
        DataUtilities = new DataUtilities(Data);
        ObjectInteraction = new ObjectInteraction(Data, this);
        Landing = new Landing(Data, this, () => _actionFsm.OnLand());
        
        _carry = new Carry(Data);
        _death = new Death(Data);
        _water = new Water(Data, this);
        _status = new Status(Data, this);
        _palette = new Palette(Data);
        _actionFsm = new ActionFsm(Data);
        _physicsCore = new PhysicsCore(Data);
        _angleRotation = new AngleRotation(Data);
        _collisionBoxes = new CollisionBoxes(Data);
        _initialization = new Initialization(Data);
        
        Recorder.ResizeAll();
        Scene.Instance.Players.Add(this);

        if (Data.Id > 0)
        {
            _cpuModule = new CpuModule(Data, this);
        }
        else
        {
#if !DEBUG
            if (!SharedData.IsDebugModeEnabled) return;
#endif
            _debugMode = new DebugMode(this);
        }
    }
    
    public ActionFsm.States Action
    {
        get => _actionFsm.State;
        set => _actionFsm.State = value;
    }

    public void Init()
    {
        _actionFsm.State = ActionFsm.States.Default;
        _initialization.Init();
        _initialization.Spawn();
        Recorder.Fill();
        Data.Node.Sprite.Animate(this);
    }

    public void ExitTree()
    {
        Scene.Instance.Players.Remove(this);
        Recorder.ResizeAll();
    }

    public void Process()
    {
        Data.Input.Update(Data.Id);
        
        if (_debugMode != null && Data.Death.State == Death.States.Wait)
        {
            if (_debugMode.Update(Data.Input)) return;
        }
        
        _cpuModule?.Process();
        _death.Process();
		
        if (Data.Movement.IsControlRoutineEnabled)
        {
            RunControlRoutine();
        }
        
        if (!Data.Death.IsDead)
        {
            _water.Process();
            _status.Update();
            _collisionBoxes.Update();
        }
		
        Recorder.Record();
        _angleRotation.Process();
        _palette.Process();
        
        Data.Node.Sprite.Animate(this);
    }
    
    private void RunControlRoutine()
    {
        Data.Physics.Update(Data.Water.IsUnderwater, Data.Super.IsSuper, Data.Node.Type, Data.Item.SpeedTimer);
        
        _actionFsm.EarlyPerform();
        _actionFsm.Perform();
        
        if (Data.Movement.IsCorePhysicsSkipped)
        {
            Data.Movement.IsCorePhysicsSkipped = false;
            return;
        }
        
        _physicsCore.ProcessCorePhysics();
        
        _actionFsm.LatePerform();
        _carry.Process();
    }
}
