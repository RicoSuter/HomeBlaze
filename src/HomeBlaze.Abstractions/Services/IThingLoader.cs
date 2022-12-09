namespace HomeBlaze.Abstractions.Services
{
    public interface IThingStorage
    {
        Task<IThing> ReadThingAsync(CancellationToken cancellationToken);

        Task WriteThingAsync(IThing thing, CancellationToken cancellationToken);

        T CloneThing<T>(T thing) where T : IThing;

        void PopulateThing<T>(T source, T target) where T : IThing;
    }
}