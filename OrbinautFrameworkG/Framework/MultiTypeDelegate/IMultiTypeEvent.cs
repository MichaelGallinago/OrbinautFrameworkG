namespace OrbinautFrameworkG.Framework.MultiTypeDelegate;

public interface IMultiTypeEvent<in T> where T : IBaseTypeDelegate
{
    void Subscribe(T subscriber);
    void Unsubscribe(T subscriber);
}
