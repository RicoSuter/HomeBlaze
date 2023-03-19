namespace HomeBlaze.Abstractions
{
    public interface IThingMetadata
    {
        bool CanDelete { get; }

        bool CanClone { get; }

        bool CanEdit { get; }
    }
}