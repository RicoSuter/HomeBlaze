namespace HomeBlaze.Abstractions.Services
{
    public interface ITypeManager
    {
        Type[] ThingTypes { get; }

        Type[] ThingInterfaces { get; }

        Type[] EventTypes { get; }

        Task InitializeAsync(CancellationToken cancellationToken);
    }
}