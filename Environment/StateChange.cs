public interface IStateChange<T1, T2> 
{
    public T1 Type { get; }
    public T2 State { get; }
}

