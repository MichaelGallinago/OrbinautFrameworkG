using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OrbinautFrameworkG.Framework.MultiTypeDelegate;

public abstract class AbstractMultiTypeDelegate<T>(int capacity)
{
    protected readonly HashSet<T> Subscribers = new(capacity);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Subscribers.Clear();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Subscribe(T subscriber) => Subscribers.Add(subscriber);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unsubscribe(T subscriber) => Subscribers.Remove(subscriber);
}

public class MultiTypeDelegate<T>(int capacity = 16) : AbstractMultiTypeDelegate<T>(capacity), 
    IMultiTypeEvent<T> where T : ITypeDelegate
{
    public void Invoke()
    {
        foreach (T subscriber in Subscribers)
        {
            subscriber.Invoke();
        }
    }
    
    public void InvokeMultiThread()
    {
        Parallel.ForEach(Subscribers, subscriber => subscriber.Invoke());
    }
}

public class MultiTypeDelegate<T, T1>(int capacity = 16) : AbstractMultiTypeDelegate<T>(capacity), 
    IMultiTypeEvent<T> where T : ITypeDelegate<T1>
{
    public void Invoke(T1 t1)
    {
        foreach (T subscriber in Subscribers)
        {
            subscriber.Invoke(t1);
        }
    }
}

public class MultiTypeDelegate<T, T1, T2>(int capacity = 16) : AbstractMultiTypeDelegate<T>(capacity), 
    IMultiTypeEvent<T> where T : ITypeDelegate<T1, T2>
{
    public void Invoke(T1 t1, T2 t2)
    {
        foreach (T subscriber in Subscribers)
        {
            subscriber.Invoke(t1, t2);
        }
    }
}

public class MultiTypeDelegate<T, T1, T2, T3>(int capacity = 16) : AbstractMultiTypeDelegate<T>(capacity), 
    IMultiTypeEvent<T> where T : ITypeDelegate<T1, T2, T3>
{
    public void Invoke(T1 t1, T2 t2, T3 t3)
    {
        foreach (T subscriber in Subscribers)
        {
            subscriber.Invoke(t1, t2, t3);
        }
    }
}

public class MultiTypeDelegate<T, T1, T2, T3, T4>(int capacity = 16) : AbstractMultiTypeDelegate<T>(capacity), 
    IMultiTypeEvent<T> where T : ITypeDelegate<T1, T2, T3, T4>
{
    public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
    {
        foreach (T subscriber in Subscribers)
        {
            subscriber.Invoke(t1, t2, t3, t4);
        }
    }
}
