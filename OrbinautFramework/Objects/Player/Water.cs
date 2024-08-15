using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Modules;
using OrbinautFramework3.Objects.Spawnable.Shield;
using static OrbinautFramework3.Objects.Player.ActionFsm;

namespace OrbinautFramework3.Objects.Player;

public struct Water(PlayerData data)
{
    public void Process()
	{
		if (Stage.Local == null || !Stage.Local.IsWaterEnabled) return;

		if (Dive()) return;
		if (UpdateAirTimer()) return;
		Leave();
	}

	private bool Dive()
	{
		if (data.Water.IsUnderwater) return false;
		
		if ((int)data.Node.Position.Y < Stage.Local.WaterLevel || data.Damage.IsHurt) return true;
		
		data.Water.IsUnderwater = true;
		data.Water.AirTimer = Constants.DefaultAirTimer;

		ResetGravityOnEdge();

		SpawnSplash();
		
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		
		data.Movement.Velocity.Vector *= new Vector2(0.5f, 0.25f);
		
		RemoveShieldUnderwater();
		
		return false;
	}

	private void ResetGravityOnEdge()
	{
		if (data.State is States.Flight or States.GlideAir or States.GlideGround) return;
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

		SharedData.PlayerShield = ShieldContainer.Types.None;
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
			data.Water.AirTimer -= Scene.Instance.ProcessSpeed;
		}

		AirTimerStates state = GetAirTimerState(data.Water.AirTimer);
		if (state == previousState) return false;
		
		switch (state)
		{
			case AirTimerStates.Alert1 or AirTimerStates.Alert2 or AirTimerStates.Alert3 when data.Id == 0:
				AudioPlayer.Sound.Play(SoundStorage.Alert);
				break;
			
			case AirTimerStates.Drowning when data.Id == 0:
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
		data.ResetState();

		data.Node.ZIndex = (int)Constants.ZIndexes.AboveForeground;
		data.Visual.Animation = Animations.Drown;
		data.Death.IsDead = true;
		data.Collision.IsObjectInteractionEnabled = false;
		data.Movement.Gravity	= GravityType.Underwater;
		data.Movement.Velocity.Vector = Vector2.Zero;
		data.Movement.GroundSpeed.Value = 0f;
		
		if (data.IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = false;
		}
	}

	private void Leave()
	{
		if ((int)data.Node.Position.Y >= Stage.Local.WaterLevel || data.Damage.IsHurt) return;

		data.Water.IsUnderwater = false;
		ResetGravityOnEdge();
		
		SpawnSplash();
		
		if (data.State == States.Flight)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
			
		if (data.Id == 0 && AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			data.ResetMusic();
		}
		
		AccelerateOnLeave();
	}

	private void AccelerateOnLeave()
	{
		if (SharedData.PhysicsType <= PhysicsCore.Types.S2 || data.Movement.Velocity.Y >= -4f)
		{
			data.Movement.Velocity.Y *= 2f;
		}
					
		if (data.Movement.Velocity.Y < -16f)
		{
			data.Movement.Velocity.Y = -16f;
		}
	}

	private void SpawnSplash()
	{
		if (data.State is States.Climb or States.GlideAir or States.GlideGround) return;
		if (CpuState == CpuStates.Respawn) return;
		
		//TODO: obj_water_splash
		//instance_create(x, c_stage.water_level, obj_water_splash);
		AudioPlayer.Sound.Play(SoundStorage.Splash);
	}
}
