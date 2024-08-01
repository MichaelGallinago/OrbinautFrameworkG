using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Player.Physics.StateChangers;
using OrbinautFramework3.Objects.Player.PlayerActions;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.PlayerConstants;

namespace OrbinautFramework3.Objects.Player;

public sealed partial class Player : Node2D, ICullable
{
	public ICullable.Types CullingType { get; }
	
	private readonly DebugMode _debugMode = new();
	public PlayerData Data { get; }
	public IMemento Memento { get; }

	[Export] private PlayerAnimatedSprite _sprite;
	[Export] private ShieldContainer _shield;
	[Export] private SpawnTypes _spawnType;
	[Export] private Types _uniqueType;
	[Export] private PackedScene _packedTail;
	private Tail _tail;

	private CpuData _cpuData = new();
	private Landing _landing = new();
	private ObjectInteraction _objectInteraction = new();
	private PhysicsData _physicsData = new();
	private Water _water = new();
	private Status _status = new();
	private Death _death = new();
	private SpinDash _spinDash = new();
	private Dash _dash = new();
	private Jump _jump = new();
	private Rotation _rotation = new();

	public Player()
	{
		Data = new PlayerData(this);
		Data.TypeChanged += OnTypeChanged;

		Memento = new BaseMemento(this);
		
		_landing.LandHandler += ReleaseDropDash;
		_landing.LandHandler += ReleaseHammerSpin;
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
			UpdateCollision();
		}
		
		Data.Record();
		_rotation.Process();
		_sprite.Animate(this);
		_tail?.Animate(this);
		ProcessPalette();
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
		Carry();
	}
	
	public void OnAttached(ICarrier carrier)
	{
		Vector2 previousPosition = carrier.CarryTargetPosition;
		
		if (Input.Press.Abc)
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 18f;
				
			IsSpinning = true;
			IsJumping = true;
			Action = new Default();
			Animation = Animations.Spin;
			Radius = RadiusSpin;
			Velocity.Vector = new Vector2(0f, PhysicParams.MinimalJumpSpeed);
					
			if (Input.Down.Left)
			{
				Velocity.X = -2f;
			}
			else if (Input.Down.Right)
			{
				Velocity.X = 2f;
			}
			
			AudioPlayer.Sound.Play(SoundStorage.Jump);
			return;
		}
		
		if (Action != Actions.Carried || carrier.Action != Actions.Flight || !Position.IsEqualApprox(previousPosition))
		{
			carrier.CarryTarget = null;
			carrier.CarryTimer = 60f;
			Action = Actions.None;
			return;
		}
		
		AttachToPlayer(carrier);
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

	private void UpdateCollision()
	{
		SetSolid(RadiusNormal.X + 1, Radius.Y);
		SetRegularHitBox();
		SetExtraHitBox();
	}

	private void SetRegularHitBox()
	{
		if (Animation != Animations.Duck || SharedData.PlayerPhysics >= PhysicsTypes.S3)
		{
			SetHitBox(8, Radius.Y - 3);
			return;
		}

		if (Type is Types.Tails or Types.Amy) return;
		SetHitBox(8, 10, 0, 6);
	}

	private void SetExtraHitBox()
	{
		switch (Animation)
		{
			case Animations.HammerSpin:
				SetHitBoxExtra(25, 25);
				break;
			
			case Animations.HammerDash:
				(int radiusX, int radiusY, int offsetX, int offsetY) = (_sprite.Frame & 3) switch
				{
					0 => (16, 16,  6,  0),
					1 => (16, 16, -7,  0),
					2 => (14, 20, -4, -4),
					3 => (17, 21,  7, -5),
					_ => throw new ArgumentOutOfRangeException()
				};
				SetHitBoxExtra(radiusX, radiusY, offsetX * (int)Facing, offsetY);
				break;
			default:
				SetHitBoxExtra(_shield.State == ShieldContainer.States.DoubleSpin ? 
					new Vector2I(24, 24) : Vector2I.Zero);
				break;
		}
	}
	
	private void ProcessPalette()
	{
		// Get player colour IDs
		ReadOnlySpan<int> colours = PlayerColourIds;
		
		int colour = PaletteUtilities.Index[colours[0]];
		UpdateSuperPalette(colour, out int colourLast, out int colourLoop, out int duration);
		UpdateRegularPalette(colour, ref colourLoop, ref colourLast, ref duration);
		
		// Apply palette
		PaletteUtilities.SetRotation(colours, colourLoop, colourLast, duration);
	}

	private ReadOnlySpan<int> PlayerColourIds => Type switch
	{
		Types.Tails => [4, 5, 6],
		Types.Knuckles => [7, 8, 9],
		Types.Amy => [10, 11, 12],
		_ => [0, 1, 2, 3]
	};

	private void UpdateSuperPalette(int colour, out int colourLast, out int colourLoop, out int duration)
	{
		switch (Type)
		{
			case Types.Sonic:
				duration = colour switch
				{
					< 2 => 19,
					< 7 => 4,
					_ => 8
				};
			    
				colourLast = 16;
				colourLoop = 7;
				break;
			
			case Types.Tails:
				duration = colour < 2 ? 28 : 12;
				colourLast = 7;
				colourLoop = 2;
				break;
			
			case Types.Knuckles:
				duration = colour switch
				{
					< 2 => 17,
					< 3 => 15,
					_ => 3
				};

				colourLast = 11;
				colourLoop = 3;
				break;
			
			case Types.Amy:
				duration = colour < 2 ? 19 : 4;
				colourLast = 11;
				colourLoop = 3;
				break;
			
			default:
				duration = 0;
				colourLast = 0;
				colourLoop = 0;
				break;
		}
	}

	private void UpdateRegularPalette(int colour, ref int colourLoop, ref int colourLast, ref int duration)
	{
		if (IsSuper) return;
		
		if (colour > 1)
		{
			if (Type == Types.Sonic)
			{
				colourLast = 21;
				duration = 4;
			}
		}
		else
		{
			colourLast = 1;
			duration = 0;
		}
	
		colourLoop = 1;
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
