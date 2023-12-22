using System.Collections.Generic;

namespace OrbinautFramework3.Framework.CommonObject;

public struct AnimationData
{
    public int Index;
    public bool SetFrame;
    public bool Sync;
    public int LoopFrame;
    public int Timer;
    public List<int> Duration;
    public List<int> Order;

    public AnimationData(int index)
    {
        Index = index;
        SetFrame = false;
        Sync = false;
        LoopFrame = 0;
        Timer = -1;
        Duration = [0];
        Order = [];
    }
}