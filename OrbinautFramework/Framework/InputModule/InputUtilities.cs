using System.Collections.Generic;
using Godot;

namespace OrbinautFramework3.Framework.InputModule;

public static class InputUtilities
{
    private const byte KeyboardId = 0;
    private const byte BaseDeviceCount = 4;
    
    public static List<Buttons> Down { get; } = [];
    public static List<Buttons> Press { get; } = [];
    public static List<bool> BlockInput { get; } = [];
    public static int DeviceCount { get; private set; } = BaseDeviceCount;
    public static bool GamepadVibration { get; set; } = true;
    
    public static bool DebugButtonDown { get; private set; }
    public static bool DebugButtonPress { get; private set; }

    private static Godot.Collections.Array<int> _gamepads;

    private static List<KeyboardControl> KeyboardControl { get; set; } =
    [
        new KeyboardControl(Key.Up, Key.Down, Key.Left, Key.Right, 
            Key.A, Key.S, Key.D, Key.Enter, Key.Space),

        new KeyboardControl(Key.None, Key.None, Key.None, Key.None,
            Key.Z, Key.X, Key.C, Key.None, Key.None)
    ];

    static InputUtilities()
    {
        for (var i = 0; i < BaseDeviceCount; i++)
        {
            Down.Add(new Buttons());
            Press.Add(new Buttons());
            BlockInput.Add(false);
        }
    }

    public static void Update()
    {
        _gamepads = Input.GetConnectedJoypads();
        for (var device = 0; device < DeviceCount; device++)
        {
            if (device == KeyboardId || _gamepads.Contains(device)) continue;
            Down.RemoveAt(device);
            Press.RemoveAt(device);
            BlockInput.RemoveAt(device);
            DeviceCount--;
        }

        var isKeyboardOnly = true;
        foreach (int device in _gamepads)
        {
            switch (device)
            {
                case >= Constants.MaxInputDevices:
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
        if (!GamepadVibration || device > Constants.MaxInputDevices || _gamepads.Contains(device)) return;
        Input.StartJoyVibration(device, weakMagnitude, strongMagnitude, duration);
    }

    private static void DeviceProcess(int device, bool isKeyboardOnly)
    {
        if (BlockInput[device]) return;
        
        var down = new Buttons();
        if (!isKeyboardOnly)
        {
            GamepadProcess(device, ref down);
        }
        
        if (device == KeyboardId)
        {
            KeyboardProcess(ref down);
        }
        
        down.Abc = down.A || down.B || down.C;

        Buttons press = Down[KeyboardId];
        press.Up = !press.Up && down.Up;
        press.Down = !press.Down && down.Down;
        press.Left = !press.Left && down.Left;
        press.Right = !press.Right && down.Right;
        press.A = !press.A && down.A;
        press.B = !press.B && down.B;
        press.C = !press.C && down.C;
        press.Start = !press.Start && down.Start;
        press.Abc = press.A || press.B || press.C;
        
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
        bool previousDebugButtonDownState = DebugButtonDown;
        foreach (KeyboardControl control in KeyboardControl)
        {
            down.Up = down.Up || Input.IsPhysicalKeyPressed(control.Up);
            down.Down = down.Down || Input.IsPhysicalKeyPressed(control.Down);
            down.Left = down.Left || Input.IsPhysicalKeyPressed(control.Left);
            down.Right = down.Right || Input.IsPhysicalKeyPressed(control.Right);
            down.A = down.A || Input.IsPhysicalKeyPressed(control.A);
            down.B = down.B || Input.IsPhysicalKeyPressed(control.B);
            down.C = down.C || Input.IsPhysicalKeyPressed(control.C);
            down.Start = down.Start || Input.IsPhysicalKeyPressed(control.Start);
            DebugButtonDown = Input.IsPhysicalKeyPressed(control.Debug);
        }

        DebugButtonPress = !previousDebugButtonDownState && DebugButtonDown;
    }

    private static void GamepadProcess(int device, ref Buttons down)
    {
        down.Up = Input.IsJoyButtonPressed(device, JoyButton.DpadUp);
        down.Down = Input.IsJoyButtonPressed(device, JoyButton.DpadDown);
        down.Left = Input.IsJoyButtonPressed(device, JoyButton.DpadLeft);
        down.Right = Input.IsJoyButtonPressed(device, JoyButton.DpadRight);
        down.A = Input.IsJoyButtonPressed(device, JoyButton.A);
        down.B = Input.IsJoyButtonPressed(device, JoyButton.B);
        down.C = Input.IsJoyButtonPressed(device, JoyButton.Y);
        down.Start = Input.IsJoyButtonPressed(device, JoyButton.Start);
    }
}
