using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;
using OrbinautFramework3.Objects.Player.PlayerActions;

namespace OrbinautFramework3.Objects.Player;

public class PlayerLogic : IStateHolder<ActionFsm.States>
{
    public CarryTarget CarryTarget { get; } = new();
    
    private readonly Recorder _recorder = new();
    private readonly DebugMode _debugMode = new();
    private readonly CpuModule _cpuModule;
    private readonly PlayerData _data;
    
    public ActionFsm.States State
    {
        get => _actionFsm.State;
        set => _actionFsm.State = value;
    }

    private Dash _dash;
    private Jump _jump = new();
    private Carry _carry = new();
    private Death _death = new();
    private Music _music = new();
    private Water _water = new();
    private Damage _damage = new();
    private Status _status = new();
    private Landing _landing = new();
    private Palette _palette = new();
    private SpinDash _spinDash = new();
    private ActionFsm _actionFsm;
    private PhysicsCore _physicsCore = new();
    private AngleRotation _angleRotation = new();
    private CollisionBoxes _collisionBoxes = new();
    private Initialization _initialization = new();
    private ObjectInteraction _objectInteraction = new();

    public PlayerLogic(IPlayerNode playerNode)
    {
        _data = new PlayerData(this, playerNode);
        
        _dash = new Dash(_data);
        _actionFsm = new ActionFsm(_data);
        
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
		
        if (_data.Physics.IsControlRoutineEnabled)
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
        _physicsCore.UpdatePhysicParameters();
		
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
        ResetGravity();
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
