namespace HomeBlaze.Abstractions
{
    public interface INotificationPublisher : IThing
    {
        Task SendNotificationAsync(string message, CancellationToken cancellationToken = default);
    }
}