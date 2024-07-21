using OrbinautFramework3.Framework.InputModule;

namespace OrbinautFramework3.Framework;

public interface IInputContainer
{
    public Buttons Press { get; }
    public Buttons Down { get; }
}
