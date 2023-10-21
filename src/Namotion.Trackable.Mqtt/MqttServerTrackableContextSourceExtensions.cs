using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using Namotion.Trackable.Sourcing;
using HomeBlaze.Mqtt;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MqttServerTrackableContextSourceExtensions
{
    public static IServiceCollection AddMqttServerTrackableSource<TTrackable>(this IServiceCollection serviceCollection)
        where TTrackable : class
    {
        return serviceCollection
            .AddSingleton<MqttServerTrackableSource<TTrackable>>()
            .AddHostedService(sp => sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>())
            .AddHostedService(sp =>
            {
                return new TrackableContextSourceBackgroundService<TTrackable>(
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>(),
                    sp.GetRequiredService<ILogger<TrackableContextSourceBackgroundService<TTrackable>>>());
            });
    }
}
