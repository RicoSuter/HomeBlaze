namespace HomeBlaze.Abstractions
{
    public interface IStateProvider
    {
        IReadOnlyDictionary<string, object?> GetState();
    }
}