using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.InputModule;

public static class InputUtilities
{
    // Input (keyboard and first joypad slot are treated as one device)
    public const byte MaxInputDevices = 4;
    
    private const byte KeyboardId = 0;
    
    public static List<Buttons> Down { get; } = new(MaxInputDevices);
    public static List<Buttons> Press { get; } = new(MaxInputDevices);
    public static List<bool> BlockInput { get; } = new(MaxInputDevices);
    public static int DeviceCount { get; private set; } = MaxInputDevices;
    public static bool JoypadRumble { get; set; } = true;
    

    private static Godot.Collections.Array<int> _joypads;

    private static List<KeyboardControl> KeyboardControl { get; } =
    [
        new KeyboardControl(Key.Up, Key.Down, Key.Left, Key.Right, 
            Key.A, Key.S, Key.Space, Key.D, Key.Enter),

        new KeyboardControl(Key.None, Key.None, Key.None, Key.None,
            Key.Z, Key.X, Key.None, Key.C, Key.None)
    ];

    static InputUtilities()
    { 
        for (var i = 0; i < MaxInputDevices; i++)
        {
            Down.Add(new Buttons());
            Press.Add(new Buttons());
            BlockInput.Add(false);
        }
    }

    public static void Update()
    {
        _joypads = Input.GetConnectedJoypads();
        for (var device = 0; device < DeviceCount; device++)
        {
            if (device == KeyboardId || _joypads.Contains(device)) continue;
            Down.RemoveAt(device);
            Press.RemoveAt(device);
            BlockInput.RemoveAt(device);
            DeviceCount--;
        }

        var isKeyboardOnly = true;
        foreach (int device in _joypads)
        {
            switch (device)
            {
                case >= MaxInputDevices:
                    continue;
                case KeyboardId:
                    isKeyboardOnly = false;
                    break;
            }

            Down.Add(new Buttons());
            Press.Add(new Buttons());
            BlockInput.Add(false);
            DeviceCount++;
            DeviceProcess(device, false);
        }

        if (isKeyboardOnly)
        {
            DeviceProcess(KeyboardId, true);
        }
    }

    public static void SetVibration(int device, float weakMagnitude, float strongMagnitude, float duration)
    {
        if (!JoypadRumble || device > MaxInputDevices || _joypads.Contains(device)) return;
        Input.StartJoyVibration(device, weakMagnitude, strongMagnitude, duration);
    }

    private static void DeviceProcess(int device, bool isKeyboardOnly)
    {
        if (BlockInput[device]) return;
        
        var down = new Buttons();
        if (!isKeyboardOnly)
        {
            JoypadProcess(device, ref down);
        }
        
        if (device == KeyboardId)
        {
            KeyboardProcess(ref down);
        }
        
        down.Aby = down.A || down.B || down.Y;

        Buttons press = Down[KeyboardId];
        press.A = !press.A && down.A;
        press.B = !press.B && down.B;
        press.X = !press.X && down.X;
        press.Y = !press.Y && down.Y;
        press.Up = !press.Up && down.Up;
        press.Down = !press.Down && down.Down;
        press.Left = !press.Left && down.Left;
        press.Right = !press.Right && down.Right;
        press.Start = !press.Start && down.Start;
        
        press.Aby = press.A || press.B || press.Y;
        
        if (down is { Left: true, Right: true })
        {
            down.Left = down.Right = press.Left = press.Right = false;
        }
        
        if (down is { Up: true, Down: true })
        {
            down.Up = down.Down = press.Up = press.Down = false;
        }
        
        Down[KeyboardId] = down;
        Press[KeyboardId] = press;
    }
    
    private static void KeyboardProcess(ref Buttons down)
    {
        foreach (KeyboardControl control in KeyboardControl)
        {
            down.A     = down.A     || Input.IsPhysicalKeyPressed(control.A);
            down.B     = down.B     || Input.IsPhysicalKeyPressed(control.B);
            down.X     = down.X     || Input.IsPhysicalKeyPressed(control.X);
            down.Y     = down.Y     || Input.IsPhysicalKeyPressed(control.Y);
            down.Up    = down.Up    || Input.IsPhysicalKeyPressed(control.Up);
            down.Down  = down.Down  || Input.IsPhysicalKeyPressed(control.Down);
            down.Left  = down.Left  || Input.IsPhysicalKeyPressed(control.Left);
            down.Right = down.Right || Input.IsPhysicalKeyPressed(control.Right);
            down.Start = down.Start || Input.IsPhysicalKeyPressed(control.Start);
        }
    }

    private static void JoypadProcess(int device, ref Buttons down)
    {
        down.A     = Input.IsJoyButtonPressed(device, JoyButton.A);
        down.B     = Input.IsJoyButtonPressed(device, JoyButton.B);
        down.X     = Input.IsJoyButtonPressed(device, JoyButton.X);
        down.Y     = Input.IsJoyButtonPressed(device, JoyButton.Y);
        down.Up    = Input.IsJoyButtonPressed(device, JoyButton.DpadUp);
        down.Down  = Input.IsJoyButtonPressed(device, JoyButton.DpadDown);
        down.Left  = Input.IsJoyButtonPressed(device, JoyButton.DpadLeft);
        down.Right = Input.IsJoyButtonPressed(device, JoyButton.DpadRight);
        down.Start = Input.IsJoyButtonPressed(device, JoyButton.Start);
    }
}
