using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CommonObject
{
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
    public List<CommonObject> EditModeObjects { get; private set; }
    
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
			case PlayerConstants.Type.Sonic:
			case PlayerConstants.Type.Knuckles:
			case PlayerConstants.Type.Global:
			case PlayerConstants.Type.GlobalAI:
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
        //TODO:
        //LifeRewards = 
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
		
		if (array_length(global.giant_ring_data) > 0)
		{
			x = global.giant_ring_data[0];
			y = global.giant_ring_data[1];
		}
		else if (Id == 0)
		{
			// TODO: Respawn CPU on the checkpoint
			
			if (array_length(global.checkpoint_data) > 0)
			{
				x = global.checkpoint_data[0];
				y = global.checkpoint_data[1] - radius_y;
			}
			
			if array_length(global.player_backup_data) > 0
			{
				ring_count	 = global.player_backup_data[0];
				barrier_type = global.player_backup_data[1];
			}
		}
		
		if player_id == 0
		{
			if c_engine.camera.target == noone
			{
				c_engine.camera.target = id;
				c_engine.camera.view_x = x - global.game_width  / 2;
				c_engine.camera.view_y = y - global.game_height / 2 + 16;
			}
			
			if global.saved_barrier != 0
			{
				barrier_type = global.saved_barrier;
			
				instance_create(x, y, obj_barrier, -1,
				{
					Target_Player: id
				});
			}
		
			global.saved_barrier = 0;
			global.saved_rings = 0;
		}
		
		score_count = global.saved_score;
		ring_count = global.saved_rings;
		life_count = global.saved_lives;
		
		life_rewards = [(ring_count div 100 * 100) + 100, (score_count div 50000 * 50000) + 50000];
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Id = Players.Count;
        Players.Add(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Players.Remove(this);
    }

    protected override void BeginStep(double processSpeed)
    {
        EditModeObjects = new List<CommonObject>
        { 
            new Ring(), new GiantRing(), new ItemBox(), new Spring(), new Motobug(), new Signpost()
        };
    }

    private void EditModeInit()
    {
        switch (FrameworkData.CurrentScene)
        {
            case StageTSZ:
                // TODO: debug objects
                EditModeObjects.AddRange(new List<CommonObject>
                {
                    //obj_platform_swing_tsz, obj_platform_tsz, obj_falling_floor_tsz, obj_block_tsz
                });
                break;
        }
    }
}
