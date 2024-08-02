using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;
using OrbinautFramework3.Objects.Player.PlayerActions;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class Player : OrbinautNode, ICarried
{
	[Export] private PlayerAnimatedSprite _sprite;
	[Export] private ShieldContainer _shield;
	[Export] private SpawnTypes _spawnType;
	[Export] private Types _uniqueType;
	[Export] private PackedScene _packedTail;
	
	public PlayerData Data { get; }
	
	private readonly DebugMode _debugMode = new();
	private CpuData _cpuData = new();
	private Tail _tail;
	
	protected Landing Landing = new();
	private ObjectInteraction _objectInteraction = new();
	private PhysicsData _physicsData = new();
	private Water _water = new();
	private Status _status = new();
	private Death _death = new();
	private SpinDash _spinDash = new();
	private Dash _dash = new();
	private Jump _jump = new();
	private Rotation _rotation = new();
	private Palette _palette = new();
	private Carry _carry = new();
	private CarryTarget _carryTarget = new();
	private CollisionBoxes _collisionBoxes = new();

	public Player()
	{
		Data = new PlayerData(this);
		Data.TypeChanged += OnTypeChanged;
		
		Landing.LandHandler += ReleaseDropDash;
		Landing.LandHandler += ReleaseHammerSpin;
	}

	public override void _Ready()
	{
		base._Ready();
		Data.Spawn();
		_sprite.FrameChanged += () => Data.IsAnimationFrameChanged = true;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		PlayerData.ResizeAllRecordedData();
		Scene.Local.Players.Add(this);
	}
	
	public override void _ExitTree()
	{
		Scene.Local.Players.Remove(this);
		PlayerData.ResizeAllRecordedData();
		base._ExitTree();
	}
	
	public override void _Process(double delta)
	{
		Data.Input.Update(Data.Id);
		
		// DEBUG MODE PLAYER ROUTINE
		if (Data.DeathState == DeathStates.Wait && Data.Id == 0 && SharedData.IsDebugModeEnabled)
		{
			if (_debugMode.Update(this, Data.Input)) return;
		}
	    
		// DEFAULT PLAYER ROUTINE
		_cpuData.Process();
		_death.Process();
		
		if (Data.IsControlRoutineEnabled)
		{
			RunControlRoutine();
		}

		if (!Data.IsDead)
		{
			_water.Process();
			_status.Update();
			_collisionBoxes.Update();
		}
		
		Data.Record();
		_rotation.Process();
		_sprite.Animate(this);
		_tail?.Animate(this);
		_palette.Process();
	}

	private void RunControlRoutine()
	{
		_physicsData.UpdatePhysicParameters();
		
		if (_spinDash.Perform()) return;
		if (_dash.Perform()) return;
		if (_jump.Perform()) return;
		if (_jump.Start()) return;
		
		// Abilities logic
		Data.Action.Perform();
		
		_physicsData.ProcessCorePhysics();
		
		ProcessGlideCollision();
		_carry.Process();
	}
	
	private void SetCameraDelayX(float delay)
	{
		if (!SharedData.CdCamera && IsCameraTarget(out ICamera camera))
		{
			camera.SetCameraDelayX(delay);
		}
	}

	private void OnTypeChanged(Types newType)
	{
		switch (newType)
		{
			case Types.Tails:
				if (_tail != null) return;
				_tail = _packedTail.Instantiate<Tail>();
				AddChild(_tail);
				break;
			
			case Types.Knuckles:
				ClimbAnimationFrameNumber = _sprite.GetAnimationFrameCount(Animations.ClimbWall, newType);
				RemoveTail();
				break;
			
			default:
				RemoveTail();
				break;
		}
	}

	private void RemoveTail()
	{
		if (_tail == null) return;
		_tail.QueueFree();
		_tail = null;
	}

	public static void IncreaseComboScore(int comboCounter = 0)
	{
		SharedData.ScoreCount += ComboScoreValues[comboCounter < 4 ? comboCounter : comboCounter < 16 ? 4 : 5];
	}

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
	}
}
