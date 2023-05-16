namespace HomeBlaze.Abstractions.Services
{
    public interface IThingStorage
    {
        Task<IGroupThing> ReadRootThingAsync(CancellationToken cancellationToken);

        Task WriteRootThingAsync(IGroupThing thing, CancellationToken cancellationToken);
    }
}