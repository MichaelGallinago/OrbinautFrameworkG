using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct Water(PlayerData data, IPlayerLogic logic)
{
    public void Process()
	{
		if (data.State == PlayerStates.Death || Zone.Instance == null || !Zone.Instance.IsWaterEnabled) return;

		if (Dive()) return;
		if (UpdateAirTimer()) return;
		Leave();
	}

	private bool Dive()
	{
		if (data.Water.IsUnderwater) return false;
		
		if ((int)data.Movement.Position.Y < Zone.Instance.WaterLevel) return true;
		
		data.Water.IsUnderwater = true;
		data.Water.AirTimer = Constants.DefaultAirTimer;

		ResetGravityOnEdge();

		SpawnSplash();
		
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		
		data.Movement.Velocity *= new Vector2(0.5f, 0.25f);
		
		RemoveShieldUnderwater();
		
		return false;
	}

	private void ResetGravityOnEdge()
	{
		if (logic.Action is States.Flight or States.GlideAir or States.GlideGround) return;
		data.ResetGravity();
	}

	private void RemoveShieldUnderwater()
	{
		if (data.Id != 0 && data.Node.Shield.Type is not (ShieldContainer.Types.Fire or ShieldContainer.Types.Lightning)) return;
		
		if (data.Node.Shield.Type == ShieldContainer.Types.Lightning)
		{
			//TODO: obj_water_flash
			//instance_create(x, y, obj_water_flash);
		}
		else if (data.Node.Shield.Type == ShieldContainer.Types.Fire)
		{
			//TODO: obj_explosion_dust
			//instance_create(x, c_stage.water_level, obj_explosion_dust, { MakeSound: false });
		}

		data.Node.Shield.Type = ShieldContainer.Types.None;
	}

	private enum AirTimerStates : byte
	{
		None, Alert1, Alert2, Alert3, Drowning, Drown
	}

	private bool UpdateAirTimer()
	{
		if (data.Node.Shield.Type == ShieldContainer.Types.Bubble) return false;

		AirTimerStates previousState = GetAirTimerState(data.Water.AirTimer);
		if (data.Water.AirTimer > 0f)
		{
			data.Water.AirTimer -= Scene.Instance.Speed;
		}

		AirTimerStates state = GetAirTimerState(data.Water.AirTimer);
		if (state == previousState) return false;
		
		switch (state)
		{
			case AirTimerStates.Alert1 or AirTimerStates.Alert2 or AirTimerStates.Alert3 when !logic.ControlType.IsCpu:
				AudioPlayer.Sound.Play(SoundStorage.Alert);
				break;
			
			case AirTimerStates.Drowning when !logic.ControlType.IsCpu:
				AudioPlayer.Music.Play(MusicStorage.Drowning);
				break;
			
			case AirTimerStates.Drown:
				ProcessDrown();
				return true;
		}

		return false;
	}

	private static AirTimerStates GetAirTimerState(float value) => value switch
	{
		> 1500f => AirTimerStates.None,
		> 1200f => AirTimerStates.Alert1,
		> 900f => AirTimerStates.Alert2,
		> 720f => AirTimerStates.Alert3,
		> 0f => AirTimerStates.Drowning,
		_ => AirTimerStates.Drown
	};

	private void ProcessDrown()
	{
		AudioPlayer.Sound.Play(SoundStorage.Drown);
		logic.ResetData();
		logic.Action = States.Default;

		data.Visual.ZIndex = (int)Constants.ZIndexes.AboveForeground;
		data.Visual.Visible = true;
		
		data.Sprite.Animation = Animations.Drown;
		data.State = PlayerStates.Death;

		MovementData movement = data.Movement;
		movement.Gravity = GravityType.Underwater;
		movement.Velocity = Vector2.Zero;
		movement.GroundSpeed = 0f;
		
		if (data.Node.IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = false;
		}
	}

	private void Leave()
	{
		if ((int)data.Movement.Position.Y >= Zone.Instance.WaterLevel) return;

		data.Water.IsUnderwater = false;
		ResetGravityOnEdge();
		
		SpawnSplash();
		
		if (logic.Action == States.Flight)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
			
		if (!logic.ControlType.IsCpu && AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			data.ResetMusic();
		}
		
		AccelerateOnLeave();
	}

	private void AccelerateOnLeave()
	{
#if (S1_PHYSICS || CD_PHYSICS || S2_PHYSICS)
		if (data.Movement.Velocity.Y >= -4f)
		{
			data.Movement.Velocity.Y *= 2f;
		}
#endif
					
		if (data.Movement.Velocity.Y < -16f)
		{
			data.Movement.Velocity.Y = -16f;
		}
	}

	private void SpawnSplash()
	{
		if (logic.Action is States.Climb or States.GlideAir or States.GlideGround) return;
		if (data.Cpu.State == CpuLogic.States.Respawn || data.Movement.Velocity.Y == 0f) return;
		
		//TODO: obj_water_splash
		//instance_create(x, c_stage.water_level, obj_water_splash);
		AudioPlayer.Sound.Play(SoundStorage.Splash);
	}
}
