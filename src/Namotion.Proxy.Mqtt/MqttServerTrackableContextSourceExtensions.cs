﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Namotion.Trackable.Sources;
using Namotion.Trackable.Mqtt;
using Namotion.Proxy.Sources;
using Namotion.Proxy;

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MqttServerTrackableContextSourceExtensions
{
    public static IServiceCollection AddMqttServerProxySource<TProxy>(
        this IServiceCollection serviceCollection, string sourceName, string? pathPrefix = null)
        where TProxy : IProxy
    {
        return serviceCollection
            .AddSingleton(sp =>
            {
                var context = sp.GetRequiredService<TProxy>().Context ??
                    throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

                var sourcePathProvider = new AttributeBasedSourcePathProvider(
                    sourceName, context, pathPrefix);

                return new MqttServerTrackableSource<TProxy>(
                    context,
                    sourcePathProvider,
                    sp.GetRequiredService<ILogger<MqttServerTrackableSource<TProxy>>>());
            })
            .AddSingleton<IHostedService>(sp => sp.GetRequiredService<MqttServerTrackableSource<TProxy>>())
            .AddSingleton<IHostedService>(sp =>
            {
                var context = sp.GetRequiredService<TProxy>().Context ??
                    throw new InvalidOperationException($"Context is not set on {nameof(TProxy)}.");

                return new ProxySourceBackgroundService<TProxy>(
                    sp.GetRequiredService<MqttServerTrackableSource<TProxy>>(),
                    context,
                    sp.GetRequiredService<ILogger<ProxySourceBackgroundService<TProxy>>>());
            });
    }
}
