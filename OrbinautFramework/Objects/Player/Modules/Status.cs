using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Modules;

public struct Status(PlayerData data)
{
    public void Update()
    {
        CreateSkidDust();
        FlickAfterGettingHit();
		
        data.Item.SpeedTimer = UpdateItemTimer(data.Item.SpeedTimer, MusicStorage.HighSpeed);
        data.Item.InvincibilityTimer = UpdateItemTimer(data.Item.InvincibilityTimer, MusicStorage.Invincibility);

        UpdateSuperForm();

        DamageData damage = data.Damage;
        damage.IsInvincible = damage.InvincibilityTimer > 0f || data.Item.InvincibilityTimer > 0f || damage.IsHurt || 
            data.Super.IsSuper || data.Node.Shield.State == ShieldContainer.States.DoubleSpin;
		
        KillPlayerOnTimeLimit();
    }

	private void CreateSkidDust()
	{
		if (data.Visual.Animation != Animations.Skid) return;
		
		//TODO: fix loop on stutter (maybe PreviousProcessSpeed?)
		if (data.Visual.DustTimer % 4f < Scene.Instance.ProcessSpeed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		data.Visual.DustTimer += Scene.Instance.ProcessSpeed;
	}

	private void FlickAfterGettingHit()
	{
		if (data.Damage.InvincibilityTimer <= 0f || data.Damage.IsHurt) return;
		data.Node.Visible = ((int)data.Damage.InvincibilityTimer & 4) > 0 || data.Damage.InvincibilityTimer <= 0f;
		data.Damage.InvincibilityTimer -= Scene.Instance.ProcessSpeed;
	}
	
	private float UpdateItemTimer(float timer, AudioStream itemMusic)
	{
		if (timer <= 0f) return 0f;
		timer -= Scene.Instance.ProcessSpeed;
		
		if (timer > 0f) return timer;
		timer = 0f;

		if (data.Id == 0 && AudioPlayer.Music.IsPlaying(itemMusic))
		{
			data.ResetMusic();
		}
		
		return timer;
	}

	private void UpdateSuperForm()
	{
		if (!data.Super.IsSuper) return;

		float newSuperTimer = data.Super.Timer - Scene.Instance.ProcessSpeed;
		if (newSuperTimer > 0f)
		{
			data.Super.Timer = newSuperTimer;
			return;
		}

		if (--SharedData.PlayerRings > 0)
		{
			data.Super.Timer = 61f;
			return;
		}
		
		SharedData.PlayerRings = 0;
		data.Damage.InvincibilityTimer = 1f;
		data.Super.Timer = 0f;
		
		data.ResetMusic();
	}

	private void KillPlayerOnTimeLimit()
	{
		if (data.Id == 0 && Scene.Instance.Time >= 36000f)
		{
			Kill();
		}
	}
}
