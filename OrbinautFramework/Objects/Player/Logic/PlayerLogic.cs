using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Actions;
using OrbinautFramework3.Objects.Player.Characters.Logic;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public class PlayerLogic : IPlayer, IPlayerCountObserver
{
    public Landing Landing { get; }
    public PlayerData Data { get; }
    public Recorder Recorder { get; }
    public TileCollider TileCollider { get; }
    public DataUtilities DataUtilities { get; }
    public ControlType ControlType { get; private set; }

    public ref Damage Damage => ref _damage;
    public ref ObjectInteraction ObjectInteraction => ref _objectInteraction;
    
    private Damage _damage;
    private ActionFsm _actionFsm;
    private ObjectInteraction _objectInteraction;
    
    private readonly Death _death;
    private readonly Water _water;
    private readonly Status _status;
    private readonly Palette _palette;
    private readonly PhysicsCore _physicsCore;
    private readonly AngleRotation _angleRotation;
    private readonly CollisionBoxes _collisionBoxes;
    private readonly Initialization _initialization;
    
    public PlayerLogic(IPlayerNode playerNode, IPlayerSprite sprite)
    {
        Data = new PlayerData(playerNode, sprite);
        
        Recorder = new Recorder(Data);
        TileCollider = new TileCollider();
        DataUtilities = new DataUtilities(Data);
        Landing = new Landing(Data, this, () => _actionFsm.OnLand());
        
        Damage = new Damage(Data, this);
        ObjectInteraction = new ObjectInteraction(Data, this);
        
        _death = new Death(Data, this);
        _water = new Water(Data, this);
        _status = new Status(Data, this);
        _palette = new Palette(Data);
        _physicsCore = new PhysicsCore(Data, this);
        _angleRotation = new AngleRotation(Data);
        _collisionBoxes = new CollisionBoxes(Data, this);
        _initialization = new Initialization(Data);
    }
    
    public void SetDependencies(CharacterDependencyGenerator dependencies)
    {
        ControlType = new ControlType(this, dependencies.Cpu) { IsCpu = Data.Id >= SharedData.RealPlayerCount };
        _actionFsm = new ActionFsm(Data, this, dependencies.Flight);
    }
    
    public ActionFsm.States Action
    {
        get => _actionFsm.State;
        set => _actionFsm.State = value;
    }

    public virtual void Init()
    {
        _actionFsm.State = ActionFsm.States.Default;
        _initialization.Init();
        _initialization.Spawn();
        Recorder.Fill();
        Data.Sprite.Process();
    }

    public void Process()
    {
        Data.Input.Update(Data.Id);
        
        ProcessState();
        
        _collisionBoxes.Update();
        _palette.Process();
    }

    private void ProcessState()
    {
        if (Data.Death.State == Death.States.Wait && ControlType.SwitchDebugMode()) return;
        
        switch (Data.State)
        {
            case PlayerStates.NoControl:
                ProcessEarlyControl();
                ProcessLateControl();
                break;
            
            case PlayerStates.Control:
                ProcessEarlyControl();
                RunControlRoutine();
                ProcessLateControl();
                break;
            
            case PlayerStates.Hurt: ProcessHurtState(); break;
            case PlayerStates.Death: ProcessDeathState(); break;
            case PlayerStates.DebugMode: ControlType.UpdateDebugMode(); break;
            case PlayerStates.Respawn: ProcessRespawnState(); break;
        }
    }

    private void ProcessEarlyControl()
    {
        Data.Physics.Update(Data.Water.IsUnderwater, Data.Super.IsSuper, Data.Node.Type, Data.Item.SpeedTimer);
        ControlType.UpdateCpu();
    }

    protected virtual void ProcessLateControl()
    {
        _water.Process();
        _status.Update();
        _angleRotation.Process();
        Data.Sprite.Process();
        Recorder.Record();
    }

    private void ProcessHurtState()
    {
        _physicsCore.CameraBounds.Match();
        _physicsCore.Position.UpdateAir();
        _physicsCore.Collision.Air.Collide();
        _angleRotation.Process();
        Data.Sprite.Process();
        Recorder.Record();
    }
    
    private void ProcessDeathState()
    {
        _death.Process();
        _physicsCore.Position.UpdateAir();
        _angleRotation.Process();
        Data.Sprite.Process();
        Recorder.Record();
    }
    
    private void ProcessRespawnState()
    {
        if (Data.IsInCamera(out ICamera camera) && camera.IsMoved)
        {
            Data.State = PlayerStates.Control;
        }
    }
    
    private void RunControlRoutine()
    {
        _actionFsm.EarlyPerform();
        _actionFsm.Perform();
        
        if (Data.Movement.IsCorePhysicsSkipped)
        {
            Data.Movement.IsCorePhysicsSkipped = false;
            return;
        }
        
        _physicsCore.ProcessCorePhysics();
        
        _actionFsm.LatePerform();
    }

    public void OnPlayerCountChanged(int count) => Recorder.Resize(count);
}
