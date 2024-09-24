using OrbinautFrameworkG.Framework.MultiTypeDelegate;

namespace OrbinautFrameworkG;

public interface IPlayerCountObserver : ITypeDelegate<int>
{
    void ITypeDelegate<int>.Invoke(int t1) => OnPlayerCountChanged(t1);
    void OnPlayerCountChanged(int count);
}
