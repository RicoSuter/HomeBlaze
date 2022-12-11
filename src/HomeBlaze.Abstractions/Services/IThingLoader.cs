namespace HomeBlaze.Abstractions.Services
{
    public interface IThingStorage
    {
        Task<IRootThing> ReadRootThingAsync(CancellationToken cancellationToken);

        Task WriteRootThingAsync(IRootThing thing, CancellationToken cancellationToken);

        T CloneThing<T>(T thing) where T : IThing;

        void PopulateThing<T>(T source, T target) where T : IThing;
    }
}