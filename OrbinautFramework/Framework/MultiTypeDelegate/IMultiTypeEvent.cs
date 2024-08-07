namespace OrbinautFramework3.Framework.MultiTypeDelegate;

public interface IMultiTypeEvent<in T> where T : ITypeDelegate
{
    void Subscribe(T subscriber);
    void Unsubscribe(T subscriber);
}
