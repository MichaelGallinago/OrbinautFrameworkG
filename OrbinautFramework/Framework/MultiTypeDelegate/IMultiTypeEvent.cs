namespace OrbinautFramework3.Framework.MultiTypeDelegate;

public interface IMultiTypeEvent<in T>
{
    void Subscribe(T subscriber);
    void Unsubscribe(T subscriber);
}
