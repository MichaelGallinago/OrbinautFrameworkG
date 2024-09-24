using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public readonly struct Status(PlayerData data, IPlayerLogic logic)
{
    public void Update()
    {
        CreateSkidDust();
        FlickAfterGettingHit();
		
        data.Item.SpeedTimer = UpdateItemTimer(data.Item.SpeedTimer, MusicStorage.HighSpeed);
        data.Item.InvincibilityTimer = UpdateItemTimer(data.Item.InvincibilityTimer, MusicStorage.Invincibility);

        UpdateSuperForm();
        
        data.Damage.IsInvincible = data.Damage.InvincibilityTimer > 0f || data.Item.InvincibilityTimer > 0f ||
                                   data.Super.IsSuper || data.Node.Shield.State == ShieldContainer.States.DoubleSpin;
		
        KillPlayerOnTimeLimit();
    }

	private void CreateSkidDust()
	{
		if (data.Sprite.Animation != Animations.Skid) return;
		
		//TODO: fix loop on stutter (maybe PreviousProcessSpeed?)
		if (data.Visual.DustTimer % 4f < Scene.Instance.Speed)
		{
			// TODO: make obj_dust_skid
			//instance_create(x, y + Radius.Y, obj_dust_skid);
		}
		data.Visual.DustTimer += Scene.Instance.Speed;
	}

	private void FlickAfterGettingHit()
	{
		DamageData damage = data.Damage;
		float timer = damage.InvincibilityTimer;
		if (timer <= 0f) return;

		damage.InvincibilityTimer -= Scene.Instance.Speed;
		data.Visual.Visible = damage.InvincibilityTimer <= 0f || ((int)timer & 4) > 0;
	}
	
	private float UpdateItemTimer(float timer, AudioStream itemMusic)
	{
		if (timer <= 0f) return 0f;
		timer -= Scene.Instance.Speed;
		
		if (timer > 0f) return timer;
		timer = 0f;

		if (AudioPlayer.Music.IsPlaying(itemMusic))
		{
			data.ResetMusic();
		}
		
		return timer;
	}

	private void UpdateSuperForm()
	{
		if (!data.Super.IsSuper) return;

		float newSuperTimer = data.Super.Timer - Scene.Instance.Speed;
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
		
		if (!AudioPlayer.Music.IsPlaying(MusicStorage.Drowning))
		{
			data.ResetMusic();
		}
	}

	private void KillPlayerOnTimeLimit()
	{
		if (!logic.ControlType.IsCpu && Scene.Instance.Time >= 36000f)
		{
			logic.Kill();
		}
	}
}
