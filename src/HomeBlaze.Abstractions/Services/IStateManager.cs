namespace HomeBlaze.Abstractions.Services
{
    public interface IStateManager
    {
        Task<(DateTimeOffset, TState?)[]> ReadStateAsync<TState>(
            string thingId,
            string propertyName,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken);
    }
}