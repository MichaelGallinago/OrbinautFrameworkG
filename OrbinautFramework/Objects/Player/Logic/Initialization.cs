using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Player.Sprite;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Logic;

public readonly struct Initialization(PlayerData data)
{
    public void Init()
    {
        data.Collision.Init(data.Node.Type);
        
        data.Cpu.Init();
        data.Item.Init();
        data.Carry.Init();
        data.Death.Init();
        data.Super.Init();
        data.Water.Init();
        data.Damage.Init();
        data.Visual.Init();
        data.Movement.Init();
        
        data.Input.Clear();
        data.Input.NoControl = false;
		
        data.Node.Visible = true;
        data.Node.Shield.State = ShieldContainer.States.None;
        data.Node.RotationDegrees = 0f;
        
        data.Sprite.Animation = Animations.Idle;
    }
	
    public void Spawn()
    {
        if (SharedData.GiantRingData != null)
        {
            data.Node.Position = SharedData.GiantRingData.Position;
        }
        else
        {
            if (SharedData.CheckpointData != null)
            {
                data.Node.Position = SharedData.CheckpointData.Position;
            }
            data.Node.Position -= new Vector2(0, data.Collision.Radius.Y + 1);
        }
		
        if (data.Id == 0 && SharedData.PlayerShield != ShieldContainer.Types.None)
        {
            // TODO: create shield
            data.Node.Shield.State = ShieldContainer.States.Active;
            //instance_create(x, y, obj_shield, { TargetPlayer: id });
        }
    }
}
