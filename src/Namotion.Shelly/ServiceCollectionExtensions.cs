using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Shelly;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NamotionShellyServiceCollectionExtensions
    {
        public static IServiceCollection AddShellyDevice(this IServiceCollection services, 
            string name, Action<ShellyDevice>? configure = null)
        {
            services
                .AddHttpClient()
                .TryAddSingleton<IThingManager>(NullThingManager.Instance);

            return services
                .AddKeyedSingleton(name, (sp, key) =>
                {
                    var device = new ShellyDevice(
                        sp.GetRequiredService<IThingManager>(), 
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<ILogger<ShellyDevice>>());

                    configure?.Invoke(device);

                    return device;
                })
                .AddSingleton<IHostedService>(sp => sp.GetRequiredKeyedService<ShellyDevice>(name));
        }
    }
}
