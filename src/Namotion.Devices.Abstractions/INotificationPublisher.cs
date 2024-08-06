namespace HomeBlaze.Abstractions
{
    public interface INotificationPublisher
    {
        Task SendNotificationAsync(string message, CancellationToken cancellationToken = default);
    }
}