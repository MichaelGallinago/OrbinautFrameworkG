using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Player.Actions;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;

namespace OrbinautFramework3.Objects.Player.Logic;

public class PlayerLogic : IPlayer, IPlayerCountObserver
{
    public PlayerData Data { get; }
    public Recorder Recorder { get; }
    public ControlType ControlType { get; }
    public TileCollider TileCollider { get; }
    public CarryTargetLogic CarryTargetLogic { get; }
    
    public Damage Damage { get; }
    public Landing Landing { get; }
    public DataUtilities DataUtilities { get; }
    public ObjectInteraction ObjectInteraction { get; }
    
    private ActionFsm _actionFsm;
    
    //private Carry _carry; //TODO: carry
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
        CarryTargetLogic = new CarryTargetLogic(Data, this);
        ControlType = new ControlType(this) { IsCpu = Data.Id >= SharedData.RealPlayerCount };
        TileCollider = new TileCollider();
        
        Damage = new Damage(Data, this);
        DataUtilities = new DataUtilities(Data);
        ObjectInteraction = new ObjectInteraction(Data, this);
        Landing = new Landing(Data, this, () => _actionFsm.OnLand());
        
        _actionFsm = new ActionFsm(Data, this);
        
        //_carry = new Carry(Data, this); //TODO: carry
        _death = new Death(Data, this);
        _water = new Water(Data, this);
        _status = new Status(Data, this);
        _palette = new Palette(Data);
        _physicsCore = new PhysicsCore(Data, this);
        _angleRotation = new AngleRotation(Data);
        _collisionBoxes = new CollisionBoxes(Data, this);
        _initialization = new Initialization(Data);
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
            case PlayerStates.Control or PlayerStates.NoControl:
                Data.Physics.Update(Data.Water.IsUnderwater, Data.Super.IsSuper, Data.Node.Type, Data.Item.SpeedTimer);
                ControlType.UpdateCpu();
                
                if (Data.State == PlayerStates.Control)
                {
                    RunControlRoutine();
                }
                
                //_carry.Process(); TODO: carry
                _water.Process();
                _status.Update();
                _angleRotation.Process();
                Data.Sprite.Process();
                Recorder.Record();
                break;
            
            case PlayerStates.Hurt:
                _physicsCore.CameraBounds.Match();
                _physicsCore.Position.UpdateAir();
                _physicsCore.Collision.Air.Collide();
                _angleRotation.Process();
                Data.Sprite.Process();
                Recorder.Record();
                break;
            
            case PlayerStates.Death:
                _death.Process();
                _physicsCore.Position.UpdateAir();
                _angleRotation.Process();
                Data.Sprite.Process();
                Recorder.Record();
                break;
            
            case PlayerStates.DebugMode:
                ControlType.UpdateDebugMode();
                break;
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
