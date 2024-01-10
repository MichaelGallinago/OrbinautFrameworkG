using System.Collections.Generic;
using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.ObjectBase;
using OrbinautFramework3.Framework.Tiles;
using OrbinautFramework3.Objects.Spawnable.Barrier;

namespace OrbinautFramework3.Objects.Player;

public abstract partial class PlayerData : BaseObject
{
	[Export] public Types Type { get; set; }
	[Export] public SpawnTypes SpawnType;
	[Export] public PlayerAnimatedSprite Sprite { get; private set; }
	[Export] public PackedScene PackedTail { get; private set; }
	
	public static List<Player> Players { get; } = [];
	
	public int Id { get; protected set; }
	
	public Animations Animation { get; set; }
	public PhysicParams PhysicParams { get; set; }
	public Vector2I Radius;
	public Vector2I RadiusNormal { get; set; }
	public Vector2I RadiusSpin { get; set; }
	public float Gravity { get; set; }
	public Vector2 Speed { get; set; }
	public float GroundSpeed { get; set; }
	public float Angle { get; set; }
	public float SlopeGravity { get; set; }

	public Constants.TileLayers TileLayer { get; set; }
	public Constants.GroundMode GroundMode { get; set; }
	public bool StickToConvex { get; set; }
    
	public bool ObjectInteraction { get; set; }
	public bool IsGrounded { get; set; }
	public bool IsSpinning { get; set; }
	public bool IsJumping { get; set; }
	public BaseObject PushingObject { get; set; }
	public bool IsUnderwater { get; set; }
	public bool IsHurt { get; set; }
	public bool IsDead { get; set; }
	public BaseObject OnObject { get; set; }
	public bool IsSuper { get; set; }
	public bool IsInvincible { get; set; }
	public float SuperValue { get; set; }

	public Actions Action { get; set; }
	public int ActionState { get; set; }
	public float ActionValue { get; set; }
	public float ActionValue2 { get; set; }
	public bool BarrierFlag { get; set; }
	public Barrier Barrier { get; set; } //= new Barrier(this);
    
	public Constants.Direction Facing { get; set; }
	public float VisualAngle { get; set; }
	
    
	public bool IsForcedRoll { get; set; }
	public float GroundLockTimer { get; set; }
	public bool IsAirLock { get; set; }
    
	public float AirTimer { get; set; }
	public uint ComboCounter { get; set; }
	public uint ScoreCount { get; set; }
	public uint RingCount { get; set; }
	public uint LifeCount { get; set; }
	public float InvincibilityFrames { get; set; }
	public float ItemSpeedTimer { get; set; }
	public float ItemInvincibilityTimer { get; set; }
	public uint[] LifeRewards { get; set; }

	public ICarried CarryTarget { get; set; }
	public float CarryTimer { get; set; }
	public Vector2 CarryTargetPosition { get; set; }
    
	public CpuStates CpuState { get; set; }
	public float CpuTimer { get; set; }
	public float CpuInputTimer { get; set; }
	public bool IsCpuJumping { get; set; }
	public bool IsCpuRespawn { get; set; }
	public Player CpuTarget { get; set; }
    
	public RestartStates RestartState { get; set; }
	public float RestartTimer { get; set; }

	public List<RecordedData> RecordedData { get; set; } = [];
	
	public CollisionTileMap TileMap { get; set; }
	public CommonStage Stage { get; set; }
	public TileCollider TileCollider { get; set; } = new();

	public Dictionary<BaseObject, Constants.TouchState> TouchObjects { get; protected set; }

	// Edit mode
	public bool IsEditMode { get; set; }
	
	public Tail Tail { get; set; }
	
	protected readonly PlayerInput Input = new();
	
	public void ResetGravity() => Gravity = IsUnderwater ? GravityType.Underwater : GravityType.Default;
	
	public virtual void ResetState()
	{
		IsHurt = false;
		IsJumping = false;
		IsSpinning = false;
		IsGrounded = false;
		StickToConvex = false;
		
		OnObject = null;
		PushingObject = null;
		
		Radius = RadiusNormal;
		Action = Actions.None;
		GroundMode = Constants.GroundMode.Floor;
	}

	public override void Init()
	{
		/*
		// This will help to determine if this script is being called again
		var _is_respawned = variable_instance_exists(id, "player_id");
		
		
		if (SpawnType == SpawnTypes.Unique)
		{
			PlayerType = _is_respawned || PlayerType == global.player_main ? PlayerType : noone;
		}
		
		if PlayerType == noone
		{
			instance_destroy(); exit;
		}
		
		// Assign player_id
		if !_is_respawned
		{
			player_id = c_engine.player_id_count++;
		}
			
		switch PlayerType
		{
			default:
			
				radius_x_normal = 9;
				radius_y_normal = 19;
				radius_x_spin = 7;
				radius_y_spin = 14;
				
			break;
			
			case PLAYER_TAILS:
			
				radius_x_normal = 9;
				radius_y_normal = 15;
				radius_x_spin = 7;
				radius_y_spin = 14;
				
			break;
			
			case PLAYER_AMY:
			
				radius_x_normal = 9;
				radius_y_normal = 16;
				radius_x_spin = 7;
				radius_y_spin = 12;
				
			break;
		}
		
		radius_x = radius_x_normal;
		radius_y = radius_y_normal;
		
		y -= radius_y + 1;
		
		grv = GRV_DEFAULT;
		vel_x = 0;
		vel_y = 0;
		vel_ground = 0;
		angle = 0;
		
		tile_layer = TILELAYER_MAIN;
		collision_mode = 0;
		stick_to_convex = false;
		
		pushing_on = noone;
		on_object = noone;
		object_interaction = true;
		is_grounded = true;
		is_spinning = false;
		is_jumping = false;
		is_underwater = false;
		is_hurt = false;
		is_dead = false;
		is_super = false;
		is_invincible = false;
		super_value = 0;
		
		action = 0;
		action_state = 0;
		action_value = 0;
		action_value2 = 0;
		super_value = 0;
		
		barrier_state = BARRIER_STATE_NONE;
		barrier_type = BARRIER_NONE;
		
		facing = DIR_POSITIVE;
		animation = ANI_IDLE;
		visual_angle = 0;
		
		camera_view_timer = 120;
		
		forced_spin	= false;
		air_lock_flag = false;
		ground_lock_timer = 0;
		
		air_timer = AIR_VALUE_MAX;
		combo_counter = 0;
		score_count	= 0;
		ring_count = 0;
		inv_frames = 0;
		item_speed_timer = 0;
		item_inv_timer = 0;
		life_count = 0;
		life_rewards = [];
		
		carry_target = noone;
		carry_timer = 0;
		carry_target_x = 0;
		carry_target_y = 0;
		
		cpu_timer = 0;
		cpu_timer_input = 0;
		cpu_is_jumping = false;
		
		if !_is_respawned
		{
			cpu_target = noone;
			cpu_state = CPU_STATE_MAIN;
		}
		else
		{
			cpu_state = CPU_STATE_RESPAWN_INIT;
		}
		
		restart_state = 0;
		restart_timer = 0;
		
		input_press = {};
		input_down = {};
		
		if _is_respawned && variable_instance_exists(id, "ds_record_data")
		{
			ds_list_destroy(ds_record_data);
		}
		
		ds_record_data = ds_list_create();
		
		if player_id != 0
		{
			var _lead_player = player_get(player_id - 1);
			
			if _is_respawned
			{
				x = c_engine.camera.view_x + 127;
				y = _lead_player.y - 192;
			}
			else
			{
				x = _lead_player.x - 16;
				y = _lead_player.y + _lead_player.radius_y - radius_y;
			}
		}
		else if !_is_respawned
		{
			if array_length(global.giant_ring_data) > 0
			{
				x = global.giant_ring_data[0];
				y = global.giant_ring_data[1];
				
				global.giant_ring_data = [];
			}
			else if array_length(global.checkpoint_data) > 0
			{
				x = global.checkpoint_data[0];
				y = global.checkpoint_data[1] - radius_y - 1;
			}
			
			c_engine.camera.target = id;
			c_engine.camera.pos_x = x - global.game_width / 2;
			c_engine.camera.pos_y = y - global.game_height / 2 + 16;

			if global.saved_rings != 0
			{
				ring_count = global.saved_rings;
			}
		
			if global.saved_barrier != BARRIER_NONE
			{
				barrier_type = global.saved_barrier;
				
				instance_create(x, y, obj_barrier, { TargetPlayer: id });
			}
			
			score_count = global.saved_score;
			life_count = global.saved_lives;
			
			global.saved_rings = 0;
			global.saved_barrier = BARRIER_NONE;
		}
		
		if PlayerType == PLAYER_TAILS
		{
			with obj_tail
			{
				if TargetPlayer == other.id
				{
					instance_destroy();
				}
			}
			
			instance_create(x, y, obj_tail, { TargetPlayer: id, depth: depth + 1 });
		}
		
		// Apply initial animation
		scr_player_animate();\
		*/
	}
}
