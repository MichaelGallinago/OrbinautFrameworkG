using System.Collections.Generic;

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
        Duration = new List<int> {0};
        Order = new List<int>();
    }
}
