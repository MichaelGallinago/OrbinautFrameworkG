using OrbinautFramework3.Framework;
using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Objects.Player;

public class PlayerInput : IInputContainer
{
    public bool NoControl { get; set; }
    public Buttons Down { get; set; }
    public Buttons Press { get; set; }
    
    public void Update(int playerId)
    {
        if (NoControl) return;
        
        if (playerId >= InputUtilities.DeviceCount)
        {
            Clear();
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