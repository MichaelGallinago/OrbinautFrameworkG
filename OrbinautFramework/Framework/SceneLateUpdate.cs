using System;
using Godot;
using OrbinautFramework3.Audio.Player;
using OrbinautFramework3.Framework.Animations;

namespace OrbinautFramework3.Framework;

public partial class SceneLateUpdate : Node
{
    public event Action Update;
    public SceneLateUpdate() => ProcessPriority = int.MaxValue;
     
    public override void _Process(double delta)
    {
        if (Scene.Local.IsPaused) return;
        UpdateLiveRewards();
        Update?.Invoke();
    }
    
    private static void UpdateLiveRewards()
    {
        const int ringIncrement = 100;
        const int scoreIncrement = 100;
		
        if (SharedData.LifeRewards == Vector2I.Zero)
        {
            SharedData.LifeRewards = new Vector2I(
                (int)(SharedData.PlayerRings / ringIncrement * ringIncrement + ringIncrement), 
                (int)SharedData.ScoreCount / scoreIncrement * scoreIncrement + scoreIncrement);
        }
		
        if (SharedData.PlayerRings >= SharedData.LifeRewards.X && SharedData.LifeRewards.X <= 200)
        {
            AddLife(0, ringIncrement);
        }
        else if (SharedData.ScoreCount >= SharedData.LifeRewards.Y)
        {
            AddLife(1, scoreIncrement);
        }
    }

    private static void AddLife(byte rewardIndex, int rewardValue)
    {
        AudioPlayer.Music.PlayJingle(MusicStorage.ExtraLife);
        SharedData.LifeCount++;
		
        Vector2I rewards = SharedData.LifeRewards;
        rewards[rewardIndex] += rewardValue;
        SharedData.LifeRewards = rewards;
    }
}