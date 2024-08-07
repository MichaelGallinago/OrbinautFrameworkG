using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrbinautFramework3.Framework.MultiTypeDelegate;

public class MultiTypeDelegate<T>(int capacity = 16) : IMultiTypeEvent<T> where T : ITypeDelegate
{
    private readonly HashSet<T> _subscribers = new(capacity);

    public void Clear() => _subscribers.Clear();
    public void Subscribe(T subscriber) => _subscribers.Add(subscriber);
    public void Unsubscribe(T subscriber) => _subscribers.Remove(subscriber);
    
    public void Invoke()
    {
        foreach (T subscriber in _subscribers)
        {
            subscriber.Invoke();
        }
    }
    
    public void InvokeMultiThread()
    {
        Parallel.ForEach(_subscribers, subscriber => subscriber.Invoke());
    }
}
