using OrbinautFrameworkG.Framework;
using OrbinautFrameworkG.Framework.InputModule;

namespace OrbinautFrameworkG.Objects.Player.Data;

public class InputData : IInputContainer
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
