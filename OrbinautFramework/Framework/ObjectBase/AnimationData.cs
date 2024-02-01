using System.Collections.Generic;

namespace OrbinautFramework3.Framework.ObjectBase;

public struct AnimationData(int index)
{
    public int Index = index;
    public bool SetFrame = false;
    public bool Sync = false;
    public int LoopFrame = 0;
    public int Timer = -1;
    public List<int> Duration = [0];
    public List<int> Order = [];
}