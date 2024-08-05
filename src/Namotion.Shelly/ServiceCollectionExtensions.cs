using Microsoft.Extensions.Configuration;
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
                .AddHttpClient();

            return services
                .AddKeyedSingleton(name, (sp, key) =>
                {
                    var device = new ShellyDevice(
                        sp.GetRequiredService<IHttpClientFactory>(),
                        sp.GetRequiredService<ILogger<ShellyDeviceBase>>());

                    configure?.Invoke(device);

                    return device;
                })
                .AddSingleton<IHostedService>(sp => sp.GetRequiredKeyedService<ShellyDeviceBase>(name));
        }
    }
}
