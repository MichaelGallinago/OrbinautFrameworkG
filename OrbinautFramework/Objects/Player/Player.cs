using System;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;
using OrbinautFramework3.Objects.Player.PlayerActions;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class Player : OrbinautNode, ICarryTarget, IPlayer
{
	[Export] public ShieldContainer Shield { get; init; }
	[Export] private PlayerAnimatedSprite _sprite;
	[Export] public Types Type { get; init; }
	
	public IMemento Memento { get; }
	public PlayerData Data { get; }
		
	public CarryTarget CarryTarget { get; } = new();
	
	private readonly DebugMode _debugMode = new();
	private CpuModule _cpuModule = new();
	
	private Landing _landing = new();
	private ObjectInteraction _objectInteraction = new();
	private PhysicsCore _physicsCore = new();
	private Water _water = new();
	private Status _status = new();
	private Death _death = new();
	private SpinDash _spinDash = new();
	private Dash _dash = new();
	private Jump _jump = new();
	private AngleRotation _angleRotation = new();
	private Palette _palette = new();
	private Carry _carry = new();
	private CollisionBoxes _collisionBoxes = new();
	private Damage _damage = new();
	
	public Player()
	{
		Memento = new PlayerMemento(this);
		Data = new PlayerData(this);
		
		Init();
		_landing.LandHandler += () => Data.Action.OnLand();
	}

	public override void _Ready()
	{
		base._Ready();
		Spawn();
		_sprite.FrameChanged += () => Data.IsAnimationFrameChanged = true;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		PlayerData.ResizeAllRecordedData();
		Scene.Instance.Players.Add(this);
	}
	
	public override void _ExitTree()
	{
		Scene.Instance.Players.Remove(this);
		PlayerData.ResizeAllRecordedData();
		base._ExitTree();
	}
	
	public override void _Process(double delta)
	{
		Data.Input.Update(Data.Id);
		
		// DEBUG MODE PLAYER ROUTINE
		if (_death.State == Death.States.Wait && Data.Id == 0 && SharedData.IsDebugModeEnabled)
		{
			if (_debugMode.Update(this, Data.Input)) return;
		}
	    
		// DEFAULT PLAYER ROUTINE
		_cpuModule?.Process();
		_death.Process();
		
		if (Data.IsControlRoutineEnabled)
		{
			RunControlRoutine();
		}

		if (!_death.IsDead)
		{
			_water.Process();
			_status.Update();
			_collisionBoxes.Update();
		}
		
		Data.Record();
		_angleRotation.Process();
		_sprite.Animate(this);
		_palette.Process();
	}

	public void Init()
	{
		Data.Init();
		_sprite.Animate(this);
	}

	private void RunControlRoutine()
	{
		_physicsCore.UpdatePhysicParameters();
		
		if (_spinDash.Perform()) return;
		if (_dash.Perform()) return;
		if (_jump.Perform()) return;
		if (_jump.Start()) return;
		
		Data.Action.Perform();
		
		_physicsCore.ProcessCorePhysics();

		Data.Action.LatePerform();
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
	
	public void Spawn()
	{
		if (SharedData.GiantRingData != null)
		{
			Position = SharedData.GiantRingData.Position;
		}
		else
		{
			if (SharedData.CheckpointData != null)
			{
				Position = SharedData.CheckpointData.Position;
			}
			Position -= new Vector2(0, Radius.Y + 1);
		}
		
		if (Id == 0 && SharedData.PlayerShield != ShieldContainer.Types.None)
		{
			// TODO: create shield
			//instance_create(x, y, obj_shield, { TargetPlayer: id });
		}
	}
}
