using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.StaticStorages;
using OrbinautFrameworkG.Objects.Player.Data;
using OrbinautFrameworkG.Objects.Player.Sprite;
using OrbinautFrameworkG.Objects.Spawnable.Shield;

namespace OrbinautFrameworkG.Objects.Player.Logic;

public readonly struct Initialization(PlayerData data)
{
    public void Init()
    {
        IPlayerNode node = data.Node;
        data.Collision.Init(node.Type);
        data.Movement.Init(node.Position);
        data.Visual.Init(node.Scale, node.ZIndex);
        
        node.Shield.State = ShieldContainer.States.None;
        node.RotationDegrees = 0f;
        
        data.Cpu.Init();
        data.Item.Init();
        data.Death.Init();
        data.Super.Init();
        data.Water.Init();
        data.Damage.Init();
        
        data.Input.Clear();
        data.Input.NoControl = false;
        
        data.Sprite.Animation = Animations.Idle;
        
        data.State = PlayerStates.Control;
    }
	
    public void Spawn()
    {
        SetSpawnPosition();
		
        if (SharedData.SavedShields == null || SharedData.SavedShields.Length <= data.Id) return;
        data.Node.Shield.Type = SharedData.SavedShields[data.Id];
    }

    private void SetSpawnPosition()
    {
        if (SharedData.GiantRingData != null)
        {
            data.Movement.Position = SharedData.GiantRingData.Position;
            return;
        }
        
        if (SharedData.CheckpointData != null)
        {
            data.Movement.Position = SharedData.CheckpointData.Position;
        }
        data.Movement.Position.Y -= data.Collision.Radius.Y + 1;
    }
}
