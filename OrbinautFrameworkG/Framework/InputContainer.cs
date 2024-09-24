using OrbinautFrameworkG.Framework.InputModule;

namespace OrbinautFrameworkG.Framework;

public interface IInputContainer
{
    public Buttons Press { get; }
    public Buttons Down { get; }
}
