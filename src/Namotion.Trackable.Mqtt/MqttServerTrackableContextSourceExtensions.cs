using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using Namotion.Trackable.Sourcing;
using HomeBlaze.Mqtt;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MqttServerTrackableContextSourceExtensions
{
    public static IServiceCollection AddMqttServerTrackableSource<TTrackable>(
        this IServiceCollection serviceCollection, string sourceName)
        where TTrackable : class
    {
        return serviceCollection
            .AddSingleton(sp => new MqttServerTrackableSource<TTrackable>(
                sourceName, 
                sp.GetRequiredService<TrackableContext<TTrackable>>(),
                sp.GetRequiredService<ILogger<MqttServerTrackableSource<TTrackable>>>()))
            .AddHostedService(sp => sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>())
            .AddHostedService(sp =>
            {
                return new TrackableContextSourceBackgroundService<TTrackable>(
                    sourceName,
                    sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>(),
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sp.GetRequiredService<ILogger<TrackableContextSourceBackgroundService<TTrackable>>>());
            });
    }
}
