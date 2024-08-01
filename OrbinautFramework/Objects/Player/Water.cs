using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.View;
using OrbinautFramework3.Objects.Player.Physics;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public struct Water
{
	public bool IsUnderwater { get; set; }
	public float AirTimer { get; set; }
	
    public void Process()
	{
		if (Stage.Local == null || !Stage.Local.IsWaterEnabled) return;

		if (Dive()) return;
		if (UpdateAirTimer()) return;
		Leave();
	}

	private bool Dive()
	{
		if (IsUnderwater) return false;
		
		if ((int)Position.Y < Stage.Local.WaterLevel || IsHurt) return true;
		
		IsUnderwater = true;
		AirTimer = Constants.DefaultAirTimer;

		ResetGravityOnEdge();

		if (PreviousPosition.Y < Stage.Local.WaterLevel)
		{
			SpawnSplash();
		}
		
		//TODO: obj_bubbles_player
		//instance_create(x, y, obj_bubbles_player, { TargetPlayer: id });
		
		Velocity.Vector *= new Vector2(0.5f, 0.25f);
		
		RemoveShieldUnderwater();
		
		return false;
	}

	private void ResetGravityOnEdge()
	{
		if (Action != Actions.Flight && (Action != Actions.Glide || ActionState == (int)GlideStates.Fall))
		{
			ResetGravity();
		}
	}

	private void RemoveShieldUnderwater()
	{
		if (Id != 0 && _shield.Type is not (ShieldContainer.Types.Fire or ShieldContainer.Types.Lightning)) return;
		
		if (_shield.Type == ShieldContainer.Types.Lightning)
		{
			//TODO: obj_water_flash
			//instance_create(x, y, obj_water_flash);
		}
		else if (_shield.Type == ShieldContainer.Types.Fire)
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
		if (_shield.Type == ShieldContainer.Types.Bubble) return false;

		AirTimerStates previousState = GetAirTimerState(AirTimer);
		if (AirTimer > 0f)
		{
			AirTimer -= Scene.Local.ProcessSpeed;
		}

		AirTimerStates state = GetAirTimerState(AirTimer);
		if (state == previousState) return false;
		
		switch (state)
		{
			case AirTimerStates.Alert1 or AirTimerStates.Alert2 or AirTimerStates.Alert3 when Id == 0:
				AudioPlayer.Sound.Play(SoundStorage.Alert);
				break;
			
			case AirTimerStates.Drowning when Id == 0:
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
		ResetState();

		ZIndex = (int)Constants.ZIndexes.AboveForeground;
		Animation = Animations.Drown;
		IsDead = true;
		IsObjectInteractionEnabled = false;
		Gravity	= GravityType.Underwater;
		Velocity.Vector = Vector2.Zero;
		GroundSpeed.Value = 0f;
		
		if (IsCameraTarget(out ICamera camera))
		{
			camera.IsMovementAllowed = false;
		}
	}

	private void Leave()
	{
		if ((int)Position.Y >= Stage.Local.WaterLevel || IsHurt) return;

		IsUnderwater = false;
		ResetGravityOnEdge();
		
		if (PreviousPosition.Y >= Stage.Local.WaterLevel)
		{
			SpawnSplash();
		}
		
		if (Action == Actions.Flight)
		{
			AudioPlayer.Sound.Play(SoundStorage.Flight);
		}
			
		if (Id == 0 && AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			ResetMusic();
		}
		
		AccelerateOnLeave();
	}

	private void AccelerateOnLeave()
	{
		if (SharedData.PhysicsType <= PhysicsTypes.S2 || Velocity.Y >= -4f)
		{
			Velocity.Y *= 2f;
		}
					
		if (Velocity.Y < -16f)
		{
			Velocity.Y = -16f;
		}
	}

	private void SpawnSplash()
	{
		if (Action == Actions.Climb || CpuState == CpuStates.Respawn ||
		    Action == Actions.Glide && ActionState == (int)GlideStates.Fall) return;
		
		//TODO: obj_water_splash
		//instance_create(x, c_stage.water_level, obj_water_splash);
		AudioPlayer.Sound.Play(SoundStorage.Splash);
	}
}
