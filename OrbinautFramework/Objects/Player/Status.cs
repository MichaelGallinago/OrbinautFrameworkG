using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player;

public struct Status
{
    public void Update()
    {
        CreateSkidDust();
        FlickAfterGettingHit();
		
        ItemSpeedTimer = UpdateItemTimer(ItemSpeedTimer, MusicStorage.HighSpeed);
        ItemInvincibilityTimer = UpdateItemTimer(ItemInvincibilityTimer, MusicStorage.Invincibility);

        UpdateSuperForm();
		
        IsInvincible = InvincibilityTimer > 0f || ItemInvincibilityTimer > 0f || 
                       IsHurt || IsSuper || _shield.State == ShieldContainer.States.DoubleSpin;
		
        KillPlayerOnTimeLimit();
    }

	private void UpdateStatus()
	{
		CreateSkidDust();
		FlickAfterGettingHit();
		
		ItemSpeedTimer = UpdateItemTimer(ItemSpeedTimer, MusicStorage.HighSpeed);
		ItemInvincibilityTimer = UpdateItemTimer(ItemInvincibilityTimer, MusicStorage.Invincibility);

		UpdateSuperForm();
		
		IsInvincible = InvincibilityTimer > 0f || ItemInvincibilityTimer > 0f || 
		               IsHurt || IsSuper || _shield.State == ShieldContainer.States.DoubleSpin;
		
		KillPlayerOnTimeLimit();
	}
	
	private void CreateSkidDust()
	{
		if (Animation != Animations.Skid) return;
		
		//TODO: fix loop on stutter (maybe PreviousProcessSpeed?)
		if (DustTimer % 4f < Scene.Local.ProcessSpeed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		DustTimer += Scene.Local.ProcessSpeed;
	}

	private void FlickAfterGettingHit()
	{
		if (InvincibilityTimer <= 0f || IsHurt) return;
		Visible = ((int)InvincibilityTimer & 4) > 0 || InvincibilityTimer <= 0f;
		InvincibilityTimer -= Scene.Local.ProcessSpeed;
	}
	
	private float UpdateItemTimer(float timer, AudioStream itemMusic)
	{
		if (timer <= 0f) return 0f;
		timer -= Scene.Local.ProcessSpeed;
		
		if (timer > 0f) return timer;
		timer = 0f;

		if (Id == 0 && AudioPlayer.Music.IsPlaying(itemMusic))
		{
			ResetMusic();
		}
		
		return timer;
	}

	private void UpdateSuperForm()
	{
		if (!IsSuper) return;
		
		if (Action == Actions.Transform)
		{
			ActionValue -= Scene.Local.ProcessSpeed;
			if (ActionValue <= 0f)
			{
				IsObjectInteractionEnabled = true;
				IsControlRoutineEnabled = true;
				Action = Actions.None;
			}
		}

		float newSuperTimer = SuperTimer - Scene.Local.ProcessSpeed;
		if (newSuperTimer > 0f)
		{
			SuperTimer = newSuperTimer;
			return;
		}

		if (--SharedData.PlayerRings > 0)
		{
			SuperTimer = 61f;
			return;
		}
		
		SharedData.PlayerRings = 0;
		InvincibilityTimer = 1;
		SuperTimer = 0f;
		
		ResetMusic();
	}

	private void KillPlayerOnTimeLimit()
	{
		if (Id == 0 && Scene.Local.Time >= 36000f)
		{
			Kill();
		}
	}
}