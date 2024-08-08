namespace OrbinautFramework3.Objects.Player.Data;

public interface IStateHolder<T>
{
    public T State { get; set; }
}
