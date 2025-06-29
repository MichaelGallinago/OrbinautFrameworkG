using Godot;
using OrbinautFrameworkG.Audio.Player;
using OrbinautFrameworkG.Framework.MultiTypeDelegate;
using OrbinautFrameworkG.Framework.StaticStorages;

namespace OrbinautFrameworkG.Framework.SceneModule;

public partial class SceneFrameEnd : Node
{
    private readonly MultiTypeDelegate<ITypeDelegate> _process = new(256);
    public IMultiTypeEvent<ITypeDelegate> Process { get; }
    
    public SceneFrameEnd()
    {
        Process = _process;
        ProcessPriority = int.MaxValue;
    }

    public override void _Process(double delta)
    {
        if (Scene.Instance.State == Scene.States.Paused) return;
        UpdateLiveRewards();
        _process.Invoke();
    }
    
    private static void UpdateLiveRewards()
    {
        const int ringIncrement = 100;
        const int scoreIncrement = 100;
		
        if (SharedData.LifeRewards == Vector2I.Zero)
        {
            SharedData.LifeRewards = new Vector2I(
                (int)(SharedData.PlayerRings / ringIncrement * ringIncrement + ringIncrement), 
                (int)SaveData.ScoreCount / scoreIncrement * scoreIncrement + scoreIncrement);
        }
		
        if (SharedData.PlayerRings >= SharedData.LifeRewards.X && SharedData.LifeRewards.X <= 200)
        {
            AddLife(0, ringIncrement);
        }
        else if (SaveData.ScoreCount >= SharedData.LifeRewards.Y)
        {
            AddLife(1, scoreIncrement);
        }
    }

    private static void AddLife(byte rewardIndex, int rewardValue)
    {
        AudioPlayer.Music.PlayJingle(MusicStorage.ExtraLife);
        SaveData.LifeCount++;
		
        Vector2I rewards = SharedData.LifeRewards;
        rewards[rewardIndex] += rewardValue;
        SharedData.LifeRewards = rewards;
    }
}
