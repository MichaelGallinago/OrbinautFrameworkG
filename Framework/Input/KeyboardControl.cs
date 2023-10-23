using Godot;

namespace OrbinautFramework3.Framework.Input;

public struct KeyboardControl
{
    public readonly Key Up;
    public readonly Key Down;
    public readonly Key Left;
    public readonly Key Right;
    public readonly Key A;
    public readonly Key B;
    public readonly Key C;
    public readonly Key Start;
    public readonly Key Debug;

    public KeyboardControl(Key up, Key down, Key left, Key right, Key a, Key b, Key c, Key start, Key debug)
    {
        Up = up;
        Down = down;
        Left = left;
        Right = right;
        A = a;
        B = b;
        C = c;
        Start = start;
        Debug = debug;
    }
}