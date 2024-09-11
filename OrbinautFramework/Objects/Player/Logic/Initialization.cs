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
        data.Death.Init();
        data.Super.Init();
        data.Water.Init();
        data.Damage.Init();
        data.Visual.Init();
        data.Movement.Init();
        
        data.Input.Clear();
        data.Input.NoControl = false;

        IPlayerNode node = data.Node;
        data.Visual.Visible = true;
        node.Shield.State = ShieldContainer.States.None;
        node.RotationDegrees = 0f;
        
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
        data.Movement.Position -= new Vector2(0, data.Collision.Radius.Y + 1);
    }
}
