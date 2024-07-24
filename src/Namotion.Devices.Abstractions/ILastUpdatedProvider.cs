namespace HomeBlaze.Abstractions
{
    public interface ILastUpdatedProvider
    {
        DateTimeOffset? LastUpdated { get; }
    }
}
