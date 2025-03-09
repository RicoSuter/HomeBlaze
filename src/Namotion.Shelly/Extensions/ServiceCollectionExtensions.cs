using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;

using Namotion.Shelly;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class NamotionShellyServiceCollectionExtensions
{
    public static IServiceCollection AddShellyDevice(this IServiceCollection services, 
        string name, Action<ShellyDevice>? configure = null)
    {
        services
            .AddHttpClient();

        return services
            .AddKeyedSingleton(name, (sp, key) =>
            {
                var device = new ShellyDevice(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    sp.GetRequiredService<ILogger<ShellyDevice>>());

                configure?.Invoke(device);

                return device;
            })
            .AddSingleton<IHostedService>(sp => sp.GetRequiredKeyedService<ShellyDevice>(name));
    }
        
    public static ShellyDevice GetRequiredShellyDevice(this IServiceProvider serviceProvider, string name)
    {
        return serviceProvider.GetRequiredKeyedService<ShellyDevice>(name);
    }
}