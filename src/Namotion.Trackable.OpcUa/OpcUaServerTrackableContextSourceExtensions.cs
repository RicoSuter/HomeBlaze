using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using Namotion.Trackable.Sources;
using Namotion.Trackable.OpcUa;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class OpcUaServerTrackableContextSourceExtensions
{
    public static IServiceCollection AddOpcUaServerTrackableSource<TTrackable>(
        this IServiceCollection serviceCollection, string sourceName, string? pathPrefix = null)
        where TTrackable : class
    {
        return serviceCollection
            .AddSingleton(sp =>
            {
                var sourcePathProvider = new AttributeBasedSourcePathProvider(
                    sourceName, sp.GetRequiredService<TrackableContext<TTrackable>>(), pathPrefix);

                return new OpcUaServerTrackableSource<TTrackable>(
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sourcePathProvider,
                    sp.GetRequiredService<ILogger<OpcUaServerTrackableSource<TTrackable>>>());
            })
            .AddHostedService(sp => sp.GetRequiredService<OpcUaServerTrackableSource<TTrackable>>())
            .AddHostedService(sp =>
            {
                return new TrackableContextSourceBackgroundService<TTrackable>(
                    sp.GetRequiredService<OpcUaServerTrackableSource<TTrackable>>(),
                    sp.GetRequiredService<TrackableContext<TTrackable>>(),
                    sp.GetRequiredService<ILogger<TrackableContextSourceBackgroundService<TTrackable>>>());
            });
    }
}
