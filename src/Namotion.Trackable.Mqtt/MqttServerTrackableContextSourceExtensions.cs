using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using HomeBlaze.Mqtt;
using Namotion.Trackable.Sources;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MqttServerTrackableContextSourceExtensions
{
    public static IServiceCollection AddMqttServerTrackableSource<TTrackable>(
        this IServiceCollection serviceCollection, string sourceName)
        where TTrackable : class
    {
        return serviceCollection
            .AddSingleton(sp =>
            {
                var sourcePathProvider = new AttributeBasedSourcePathProvider(
                    sourceName, sp.GetRequiredService<TrackableContext<TTrackable>>());

                return new MqttServerTrackableSource<TTrackable>(
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sourcePathProvider,
                    sp.GetRequiredService<ILogger<MqttServerTrackableSource<TTrackable>>>());
            })
            .AddHostedService(sp => sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>())
            .AddHostedService(sp =>
            {
                return new TrackableContextSourceBackgroundService<TTrackable>(
                    sp.GetRequiredService<MqttServerTrackableSource<TTrackable>>(),
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sp.GetRequiredService<ILogger<TrackableContextSourceBackgroundService<TTrackable>>>());
            });
    }
}
