namespace OrbinautFrameworkG.Framework.MultiTypeDelegate;

public interface ITypeDelegate { void Invoke(); }
public interface ITypeDelegate<in T1> { void Invoke(T1 t1); }
public interface ITypeDelegate<in T1, in T2> { void Invoke(T1 t1, T2 t2); }
public interface ITypeDelegate<in T1, in T2, in T3> { void Invoke(T1 t1, T2 t2, T3 t3); }
public interface ITypeDelegate<in T1, in T2, in T3, in T4> { void Invoke(T1 t1, T2 t2, T3 t3, T4 t4); }
