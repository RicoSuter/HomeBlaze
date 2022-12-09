using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions
{
    public interface ITrigger : IThing
    {
        [Operation]
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}