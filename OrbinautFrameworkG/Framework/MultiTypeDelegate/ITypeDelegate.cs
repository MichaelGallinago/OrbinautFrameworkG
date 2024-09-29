namespace OrbinautFrameworkG.Framework.MultiTypeDelegate;

public interface IBaseTypeDelegate;

public interface ITypeDelegate : IBaseTypeDelegate
{
    void Invoke();
}

public interface ITypeDelegate<in T1> : IBaseTypeDelegate
{
    void Invoke(T1 t1);
}

public interface ITypeDelegate<in T1, in T2> : IBaseTypeDelegate
{
    void Invoke(T1 t1, T2 t2);
}

public interface ITypeDelegate<in T1, in T2, in T3> : IBaseTypeDelegate
{
    void Invoke(T1 t1, T2 t2, T3 t3);
}

public interface ITypeDelegate<in T1, in T2, in T3, in T4> : IBaseTypeDelegate
{
    void Invoke(T1 t1, T2 t2, T3 t3, T4 t4);
}
