using Godot;

namespace OrbinautFramework3.Framework.Input;

public struct KeyboardControl(Key up, Key down, Key left, Key right, Key a, Key b, Key c, Key start, Key debug)
{
    public readonly Key Up = up;
    public readonly Key Down = down;
    public readonly Key Left = left;
    public readonly Key Right = right;
    public readonly Key A = a;
    public readonly Key B = b;
    public readonly Key C = c;
    public readonly Key Start = start;
    public readonly Key Debug = debug;
}
