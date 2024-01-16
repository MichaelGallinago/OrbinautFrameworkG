using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.Input;

namespace OrbinautFramework3.Objects.Player;

public class PlayerInput : IInputContainer
{
    public Buttons Down { get; set; }
    public Buttons Press { get; set; }
    
    public void Update(int playerId)
    {
        if (playerId >= InputUtilities.DeviceCount)
        {
            Down = Press = new Buttons();
            return;
        }
	    
        Press = InputUtilities.Press[playerId];
        Down = InputUtilities.Down[playerId];
    }

    public void Set(Buttons press, Buttons down)
    {
        Press = press;
        Down = down;
    }

    public void Clear() => Down = Press = new Buttons();
}