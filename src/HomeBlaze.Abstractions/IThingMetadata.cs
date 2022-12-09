namespace HomeBlaze.Abstractions
{
    public interface IThingMetadata
    {
        bool CanDelete { get; }

        bool CanEdit { get; }
    }
}