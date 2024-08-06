using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Namotion.Trackable.Sources;
using Namotion.Proxy.Sources;
using Namotion.Proxy;
using Namotion.Proxy.OpcUa.Server;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class OpcUaProxyExtensions
{
    public static IServiceCollection AddOpcUaServerProxySource<TProxy>(
        this IServiceCollection serviceCollection,
        string sourceName,
        string? pathPrefix = null,
        string? rootName = null)
        where TProxy : IProxy
    {
        return serviceCollection.AddOpcUaServerProxySource(
            sourceName,
            sp => sp.GetRequiredService<TProxy>(),
            pathPrefix,
            rootName);
    }

    public static IServiceCollection AddOpcUaServerProxySource<TProxy>(
        this IServiceCollection serviceCollection,
        string sourceName,
        Func<IServiceProvider, TProxy> resolveProxy,
        string? pathPrefix = null,
        string? rootName = null)
        where TProxy : IProxy
    {
        return serviceCollection
            .AddSingleton(sp =>
            {
                var proxy = resolveProxy(sp);
                var context = proxy.Context ?? 
                    throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

                var sourcePathProvider = new AttributeBasedSourcePathProvider(
                    sourceName, context, pathPrefix);

                return new OpcUaServerTrackableSource<TProxy>(
                    proxy,
                    sourcePathProvider,
                    sp.GetRequiredService<ILogger<OpcUaServerTrackableSource<TProxy>>>(),
                    rootName);
            })
            .AddSingleton<IHostedService>(sp => sp.GetRequiredService<OpcUaServerTrackableSource<TProxy>>())
            .AddSingleton<IHostedService>(sp =>
            {
                var proxy = resolveProxy(sp);
                var context = proxy.Context ??
                    throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

                return new ProxySourceBackgroundService<TProxy>(
                    sp.GetRequiredService<OpcUaServerTrackableSource<TProxy>>(),
                    context,
                    sp.GetRequiredService<ILogger<ProxySourceBackgroundService<TProxy>>>());
            });
    }

    //public static IServiceCollection AddOpcUaClientProxySource<TProxy>(
    //    this IServiceCollection serviceCollection,
    //    string sourceName,
    //    string serverUrl,
    //    string? pathPrefix = null,
    //    string? rootName = null)
    //    where TProxy : IProxy
    //{
    //    return serviceCollection.AddOpcUaClientProxySource(
    //        sourceName,
    //        serverUrl,
    //        sp => sp.GetRequiredService<TProxy>(),
    //        pathPrefix,
    //        rootName);
    //}

    //public static IServiceCollection AddOpcUaClientProxySource<TProxy>(
    //    this IServiceCollection serviceCollection,
    //    string sourceName,
    //    string serverUrl,
    //    Func<IServiceProvider, TProxy> resolveProxy,
    //    string? pathPrefix = null,
    //    string? rootName = null)
    //    where TProxy : IProxy
    //{
    //    return serviceCollection
    //        .AddSingleton(sp =>
    //        {
    //            var proxy = resolveProxy(sp);
    //            var context = proxy.Context ??
    //                throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

    //            var sourcePathProvider = new AttributeBasedSourcePathProvider(
    //                sourceName, context, pathPrefix);

    //            return new OpcUaClientTrackableSource<TProxy>(
    //                proxy,
    //                serverUrl,
    //                sourcePathProvider,
    //                sp.GetRequiredService<ILogger<OpcUaClientTrackableSource<TProxy>>>(),
    //                rootName);
    //        })
    //        .AddSingleton<IHostedService>(sp => sp.GetRequiredService<OpcUaClientTrackableSource<TProxy>>())
    //        .AddSingleton<IHostedService>(sp =>
    //        {
    //            var proxy = resolveProxy(sp);
    //            var context = proxy.Context ??
    //                throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

    //            return new ProxySourceBackgroundService<TProxy>(
    //                sp.GetRequiredService<OpcUaClientTrackableSource<TProxy>>(),
    //                context,
    //                sp.GetRequiredService<ILogger<ProxySourceBackgroundService<TProxy>>>());
    //        });
    //}
}
