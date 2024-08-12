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

    private Dash _dash;
    private Jump _jump;
    private Carry _carry;
    private Death _death;
    private Water _water;
    private Damage _damage;
    private Status _status;
    private Landing _landing;
    private Palette _palette;
    private SpinDash _spinDash;
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
        _debugMode = new DebugMode(_data); // Create only if allowed
        
        _dash = new Dash(_data);
        _jump = new Jump(_data);
        _carry = new Carry(_data);
        _death = new Death(_data);
        _water = new Water(_data);
        _damage = new Damage(_data);
        _status = new Status(_data);
        _landing = new Landing(_data);
        _palette = new Palette(_data);
        _spinDash = new SpinDash(_data);
        _actionFsm = new ActionFsm(_data);
        _physicsCore = new PhysicsCore(_data);
        _angleRotation = new AngleRotation(_data);
        _collisionBoxes = new CollisionBoxes(_data);
        _initialization = new Initialization(_data);
        _objectInteraction = new ObjectInteraction(_data);
        
        _cpuModule = new CpuModule(_data);
        _landing.LandHandler += () => _actionFsm.OnLand();
    }

    public void Init()
    {
        _initialization.Init();
        _initialization.Spawn();
        _cpuModule?.Init();
        _recorder.Fill();
    }

    public void Process()
    {
        _data.Input.Update(_data.Id);
        
        if (_data.Death.State == Death.States.Wait && _data.Id == 0 && SharedData.IsDebugModeEnabled)
        {
            if (_debugMode.Update(this, _data.Input)) return;
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
        _data.Physics.Update(
            _data.Water.IsUnderwater, _data.Super.IsSuper, _data.Node.Type, _data.Item.SpeedTimer);
		
        if (_spinDash.Perform()) return;
        if (_dash.Perform()) return;
        if (_jump.Perform()) return;
        if (_jump.Start()) return;
		
        _actionFsm.Perform();
		
        _physicsCore.ProcessCorePhysics();

        _actionFsm.LatePerform();
        _carry.Process();
    }
    
    /*
    //TODO: update debug mode
    public void OnEnableEditMode()
    {
        _data.ResetGravity();
        ResetState();
        //ResetZIndex();

        Visible = true;
        IsObjectInteractionEnabled = false;
    }

    public void OnDisableEditMode()
    {
        Velocity.Vector = Vector2.Zero;
        GroundSpeed.Value = 0f;
        Animation = Animations.Move;
        IsObjectInteractionEnabled = true;
        DeathState = DeathStates.Wait;
    }*/
}
