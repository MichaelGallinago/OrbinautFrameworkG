using Godot;
using OrbinautFramework3.Framework;
using OrbinautFramework3.Objects.Player.Data;
using OrbinautFramework3.Objects.Spawnable.Shield;

namespace OrbinautFramework3.Objects.Player.Modules;

public readonly struct Initialization(PlayerData data)
{
    public void Init()
    {
        data.Collision.Init(data.PlayerNode.Type);
        
        data.Item.Init();
        data.Carry.Init();
        data.Death.Init();
        data.Super.Init();
        data.Water.Init();
        data.Damage.Init();
        data.Visual.Init();
        data.Physics.Init();
        data.Rotation.Init();
        
        data.Action.Type = Actions.Types.Default;
        
        data.Input.Clear();
        data.Input.NoControl = false;
		
        data.PlayerNode.Visible = true;
        data.PlayerNode.Shield.State = ShieldContainer.States.None;
        data.PlayerNode.RotationDegrees = 0f;
    }
	
    public void Spawn()
    {
        if (SharedData.GiantRingData != null)
        {
            data.PlayerNode.Position = SharedData.GiantRingData.Position;
        }
        else
        {
            if (SharedData.CheckpointData != null)
            {
                data.PlayerNode.Position = SharedData.CheckpointData.Position;
            }
            data.PlayerNode.Position -= new Vector2(0, data.Collision.Radius.Y + 1);
        }
		
        if (data.Id == 0 && SharedData.PlayerShield != ShieldContainer.Types.None)
        {
            // TODO: create shield
            data.PlayerNode.Shield.State = ShieldContainer.States.Active;
            //instance_create(x, y, obj_shield, { TargetPlayer: id });
        }
    }
}
