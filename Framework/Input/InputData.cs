using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class InputData : Node
{
    private const byte KeyboardId = 0;
    
    public Dictionary<int, Buttons> Down { get; }
    public Dictionary<int, Buttons> Press { get; }

    public InputData()
    {
        Down = new Dictionary<int, Buttons> { { KeyboardId, new Buttons() } };
        Press = new Dictionary<int, Buttons> { { KeyboardId, new Buttons() } };
    }

    public override void _Process(double delta)
    {
        Godot.Collections.Array<int> gamepads = Input.GetConnectedJoypads();
        foreach (int device in Down.Keys.Where(device => !gamepads.Contains(device)))
        {
            Down.Remove(device);
            Press.Remove(device);
        }

        var isKeyboardOnly = true;
        foreach (int device in gamepads)
        {
            switch (device)
            {
                case >= Constants.MaxInputDevices:
                    continue;
                case KeyboardId:
                    isKeyboardOnly = false;
                    break;
            }

            Down.TryAdd(device, new Buttons());
            Press.TryAdd(device, new Buttons());
            DeviceProcess(device, false);
        }

        if (isKeyboardOnly)
        {
            DeviceProcess(KeyboardId, true);
        }
    }

    private void DeviceProcess(int device, bool isKeyboardOnly)
    {
        var down = new Buttons();
        if (!isKeyboardOnly)
        {
            down.Up = down.Up || Input.IsJoyButtonPressed(device, JoyButton.DpadUp);
            down.Down = down.Down || Input.IsJoyButtonPressed(device, JoyButton.DpadDown);
            down.Left = down.Left || Input.IsJoyButtonPressed(device, JoyButton.DpadLeft);
            down.Right = down.Right || Input.IsJoyButtonPressed(device, JoyButton.DpadRight);
            down.A = down.A || Input.IsJoyButtonPressed(device, JoyButton.A);
            down.B = down.B || Input.IsJoyButtonPressed(device, JoyButton.B);
            down.C = down.C || Input.IsJoyButtonPressed(device, JoyButton.Y);
            down.Start = down.Start || Input.IsJoyButtonPressed(device, JoyButton.Start);
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
        
        Down[KeyboardId] = down;
        Press[KeyboardId] = press;
    }

    private static void KeyboardProcess(ref Buttons down)
    {
        foreach (KeyboardControl control in FrameworkData.KeyboardControl)
        {
            down.Up = down.Up || Input.IsPhysicalKeyPressed(control.Up);
            down.Down = down.Down || Input.IsPhysicalKeyPressed(control.Down);
            down.Left = down.Left || Input.IsPhysicalKeyPressed(control.Left);
            down.Right = down.Right || Input.IsPhysicalKeyPressed(control.Right);
            down.A = down.A || Input.IsPhysicalKeyPressed(control.A);
            down.B = down.B || Input.IsPhysicalKeyPressed(control.B);
            down.C = down.C || Input.IsPhysicalKeyPressed(control.C);
            down.Start = down.Start || Input.IsPhysicalKeyPressed(control.Start);
        }
    }
}
