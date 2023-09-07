using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CommonObject
{
	private const byte EditModeAccelerationMultiplier = 4;
	private const float EditModeAcceleration = 0.046875f;
	private const byte EditModeSpeedLimit = 16;
	
    public static List<CommonObject> Players { get; }
    
    [Export] public PlayerConstants.Type Type;

    public int Id { get; private set; }

    public Vector2I Radius { get; set; }
    public Vector2I RadiusNormal { get; set; }
    public Vector2I RadiusSpin { get; set; }
    public float Gravity { get; set; }
    public Vector2 Speed { get; set; }
    public float GroundSpeed { get; set; }
    public float Angle { get; set; }
    public float SlopeGravity { get; set; }

    public Constants.TileLayer TileLayer { get; set; }
    public Constants.GroundMode GroundMode { get; set; }
    public bool StickToConvex { get; set; }
    
    public bool ObjectInteraction { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsSpinning { get; set; }
    public bool IsJumping { get; set; }
    public bool IsPushing { get; set; }
    public bool IsUnderwater { get; set; }
    public bool IsHurt { get; set; }
    public bool IsDead { get; set; }
    public bool IsOnObject { get; set; }
    public bool IsSuper { get; set; }
    public bool IsInvincible { get; set; }
    public int SuperValue { get; set; }

    public PlayerConstants.Action Action { get; set; }
    public int ActionState { get; set; }
    public int ActionValue { get; set; }
    public int ActionValue2 { get; set; }
    public bool BarrierFlag { get; set; }
    public Constants.Barrier BarrierType { get; set; }
    
    public Constants.Direction Facing { get; set; }
    public PlayerConstants.Animation Animation { get; set; }
    public float AnimationTimer { get; set; }
    public float VisualAngle { get; set; }
    
    public int CameraViewTimer { get; set; }
    
    public bool IsForcedRoll { get; set; }
    public float GroundLockTimer { get; set; }
    public bool IsAirLock { get; set; }
    
    public float AirTimer { get; set; }
    public uint ScoreCombo { get; set; }
    public uint ScoreCount { get; set; }
    public uint RingCount { get; set; }
    public uint LifeCount { get; set; }
    public float InvincibilityFrames { get; set; }
    public float ItemSpeedTimer { get; set; }
    public float ItemInvincibilityTimer { get; set; }
    public uint[] LifeRewards { get; set; }

    public Player CarryTarget { get; set; }
    public float CarryTimer { get; set; }
    public Vector2 CarryParentPosition { get; set; }
    
    public PlayerConstants.CpuState CpuState { get; set; }
    public float CpuTimer { get; set; }
    public float CpuInputTimer { get; set; }
    public bool IsCpuJumping { get; set; }
    public bool IsCpuRespawn { get; set; }
    public Player CpuTarget { get; set; }
    
    public PlayerConstants.RestartState RestartState { get; set; }
    public float RestartTimer { get; set; }

    public Buttons InputPress { get; set; }
    public Buttons InputDown { get; set; }

    public List<RecordedData> RecordedData { get; set; }

    // Edit mode
    public bool IsEditMode { get; private set; }
    public int EditModeIndex { get; private set; }
    public float EditModeSpeed { get; private set; }
    public List<Type> EditModeObjects { get; private set; }
    
    static Player()
    {
        Players = new List<CommonObject>();
    }

    public override void _Ready()
    {
        SetBehaviour(ObjectRespawnData.BehaviourType.Unique);
        
        switch (Type)
		{
			case PlayerConstants.Type.Tails:
				RadiusNormal = new Vector2I(9, 15);
				RadiusSpin = new Vector2I(7, 14);
				break;
			case PlayerConstants.Type.Amy:
				RadiusNormal = new Vector2I(9, 16);
				RadiusSpin = new Vector2I(7, 12);
				break;
			default:
				RadiusNormal = new Vector2I(9, 19);
				RadiusSpin = new Vector2I(7, 14);
				break;
		}

        Radius = RadiusNormal;

        Position = new Vector2(Position.X, Position.Y - Radius.Y + 1);

        Gravity = GravityType.Default;
        TileLayer = Constants.TileLayer.Main;
        GroundMode = Constants.GroundMode.Floor;
        ObjectInteraction = true;
        BarrierType = Constants.Barrier.None;
        Facing = Constants.Direction.Positive;
        Animation = PlayerConstants.Animation.Idle;
        AirTimer = 1800f;
        CpuState = PlayerConstants.CpuState.Main;
        RestartState = PlayerConstants.RestartState.GameOver;
        InputPress = new Buttons();
        InputDown = new Buttons();
        RecordedData = new List<RecordedData>();

		if (Type == PlayerConstants.Type.Tails)
		{
			var tail = new Tail(this);
			AddChild(tail);
		}
		
		if (FrameworkData.GiantRingData != null)
		{
			Position = (Vector2)FrameworkData.GiantRingData;
		}
		else if (Id == 0)
		{
			// TODO: Respawn CPU on the checkpoint
			
			if (FrameworkData.CheckpointData != null)
			{
				Vector2I position = FrameworkData.CheckpointData.Position;
				position.Y -= Radius.Y;
				Position = position;
			}
			
			if (FrameworkData.PlayerBackupData != null)
			{
				RingCount = FrameworkData.PlayerBackupData.RingCount;
				BarrierType = FrameworkData.PlayerBackupData.BarrierType;
			}
		}
		
		if (Id == 0)
		{
			if (FrameworkData.SavedBarrier != Constants.Barrier.None)
			{
				BarrierType = FrameworkData.SavedBarrier;
				AddChild(new Barrier(this));
			}
		
			FrameworkData.SavedBarrier = 0;
			FrameworkData.SavedRings = 0;
		}
		
		ScoreCount = FrameworkData.SavedScore;
		RingCount = FrameworkData.SavedRings;
		LifeCount = FrameworkData.SavedLives;
		
		LifeRewards = new[] { RingCount / 100 * 100 + 100, ScoreCount / 50000 * 50000 + 50000 };
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Id = Players.Count;
        Players.Add(this);
        FrameworkData.CurrentScene.AddPlayerStep(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Players.Remove(this);
        for (int i = Id; i < Players.Count; i++)
        {
	        ((Player)Players[i]).Id--;
        }
        FrameworkData.CurrentScene.RemovePlayerStep(this);
        if (Players.Count == 0 || !IsCpuRespawn) return;
        var newPlayer = new Player()
        {
	        Type = Type,
	        Position = Players.First().Position
        };

        newPlayer.PlayerStep(FrameworkData.ProcessSpeed);
    }

    public void PlayerStep(double processSpeed)
    {
	    UpdateInput();

	    if (ProcessEditMode((float)processSpeed)) return;
	    
    }
    
    public void SetInput(Buttons inputPress, Buttons inputDown)
    {
	    InputPress = inputPress;
	    InputDown = inputDown;
    }

    public void ResetGravity()
    {
	    Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
    }
    
    public void ResetState()
    {
	    switch (Action)
	    {
		    //TODO: audio
		    case PlayerConstants.Action.PeelOut:
				//audio_stop_sfx(sfx_charge2);
				break;
		
		    case PlayerConstants.Action.Flight:
			    //audio_stop_sfx(sfx_flight);
				//audio_stop_sfx(sfx_flight2);
			    break;
	    }
	
	    IsHurt = false;
	    IsJumping = false;
	    IsSpinning = false;
	    IsPushing = false;
	    IsGrounded = false;
	    IsOnObject = false;
	
	    StickToConvex = false;
	    GroundMode = 0;
	
	    Action = PlayerConstants.Action.None;
	
	    Radius = RadiusNormal;
    }
    
    private void EditModeInit()
    {
	    EditModeObjects = new List<Type>
	    { 
		    typeof(Ring), typeof(GiantRing), typeof(ItemBox), typeof(Spring), typeof(Motobug), typeof(Signpost)
	    };
	    
        switch (FrameworkData.CurrentScene)
        {
            case StageTSZ:
                // TODO: debug objects
                EditModeObjects.AddRange(new List<Type>
                {
                    //typeof(obj_platform_swing_tsz), typeof(obj_platform_tsz), typeof(obj_falling_floor_tsz), typeof(obj_block_tsz)
                });
                break;
        }
    }

    private void UpdateInput()
    {
	    if (Id >= InputUtilities.DeviceCount)
	    {
		    SetInput(new Buttons(), new Buttons());
		    return;
	    }
	    
	    SetInput(InputUtilities.Press[Id], InputUtilities.Down[Id]);
    }

    private bool ProcessEditMode(float processSpeed)
    {
	    if (Id > 0 || !(FrameworkData.PlayerEditMode || FrameworkData.DeveloperMode)) return false;

	    var debugButton = false;
		
		// If in developer mode, remap debug button to Spacebar
		if (FrameworkData.DeveloperMode)
		{
			debugButton = InputUtilities.DebugButtonPress;
			
			if (IsEditMode)
			{
				debugButton = debugButton || InputPress.B;
			}
		}
		else
		{
			debugButton = InputPress.B;
		}
		
		if (debugButton)
		{
			if (!IsEditMode)
			{
				if (FrameworkData.CurrentScene.IsStage)
				{
					//TODO: audio
					//stage_reset_bgm();
				}
				
				ResetGravity();
				ResetState();
				ResetZIndex();

				FrameworkData.UpdateGraphics = true;
				FrameworkData.UpdateObjects = true;
				FrameworkData.UpdateTimer = true;
				FrameworkData.AllowPause = true;
				
				ObjectInteraction = false;
				
				EditModeSpeed = 0;
				IsEditMode = true;
				
				Visible = true;
			}
			else
			{
				Speed = new Vector2();
				GroundSpeed = 0f;

				Animation = PlayerConstants.Animation.Move;
				
				ObjectInteraction = true;
				IsEditMode = false;
				IsDead = false;
			}
		}
		
		// Continue if Edit mode is enabled
		if (!IsEditMode) return false;

		// Update speed and position (move faster if in developer mode)
		if (InputDown.Up || InputDown.Down || InputDown.Left || InputDown.Right)
		{
			EditModeSpeed = Mathf.Min(EditModeSpeed + (FrameworkData.DeveloperMode ? 
				EditModeAcceleration * EditModeAccelerationMultiplier : EditModeAcceleration), EditModeSpeedLimit);

			Vector2 position = Position;

			if (InputDown.Up)
			{
				position.Y -= EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Down)
			{
				position.Y += EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Left)
			{
				position.X -= EditModeSpeed * processSpeed;
			}
			
			if (InputDown.Right)
			{
				position.X += EditModeSpeed * processSpeed;
			}

			Position = position;
		}
		else
		{
			EditModeSpeed = 0;
		}

		if (InputDown.A && InputPress.C)
		{
			if (--EditModeIndex < 0)
			{
				EditModeIndex = EditModeObjects.Count - 1;
			}
		}
		else if (InputPress.A)
		{
			if (++EditModeIndex >= EditModeObjects.Count)
			{
				EditModeIndex = 0;
			}
		}
		else if (InputPress.C)
		{
			if (Activator.CreateInstance(EditModeObjects[EditModeIndex]) is not CommonObject newObject) return true;
			newObject.Scale = new Vector2(newObject.Scale.X * (sbyte)Facing, newObject.Scale.Y);
			newObject.SetBehaviour(ObjectRespawnData.BehaviourType.Delete);
			FrameworkData.CurrentScene.AddChild(newObject);
		}
		
		return true;
    }
}
