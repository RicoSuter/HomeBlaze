using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Namotion.NuGetPlugins;
using Namotion.Storage;
using Namotion.Storage.Azure.Storage.Blob;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace HomeBlaze.Host
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHomeBlaze(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMudServices();
            services.AddHotKeys2();

            var storageType = configuration.GetValue<string>("Storage:Type");
            switch (storageType)
            {
                case "AzureBlobs":
                    services
                        .AddSingleton(s => AzureBlobStorage
                            .CreateFromConnectionString(configuration.GetValue<string>("Storage:ConnectionString"))
                            .GetContainer(configuration.GetValue<string>("Storage:Container")));
                    break;

                default:
                    services
                        .AddSingleton<IBlobContainer>(s => FileSystemBlobStorage
                            .CreateWithBasePath(configuration.GetValue("Storage:Path", "Config")));
                    break;
            }

            services.AddSingleton<IThingStorage, ThingStorage>();
            services.AddSingleton<ITypeManager, TypeManager>();
            services.AddSingleton<IThingSerializer, ThingSerializer>();

            services
                .AddSingleton<IThingManager, ThingManager>()
                .AddHostedService(s => (ThingManager)s.GetRequiredService<IThingManager>());

            var seriesType = configuration.GetValue<string>("Series:Type");
            if (seriesType == "InfluxDb,Blobs")
            {
                services
                    .AddSingleton<BlobStateManager>()
                    .AddHostedService(s => s.GetRequiredService<BlobStateManager>())

                    .AddSingleton<InfluxStateManager>()
                    .AddHostedService(s => s.GetRequiredService<InfluxStateManager>())

                    .AddSingleton<IStateManager>(s => s.GetRequiredService<BlobStateManager>());
            }
            else if (seriesType == "InfluxDb")
            {
                services
                    .AddSingleton<InfluxStateManager>()
                    .AddHostedService(s => s.GetRequiredService<InfluxStateManager>())

                    .AddSingleton<IStateManager>(s => s.GetRequiredService<InfluxStateManager>());
            }
            else
            {
                services
                    .AddSingleton<BlobStateManager>()
                    .AddHostedService(s => s.GetRequiredService<BlobStateManager>())

                    .AddSingleton<IStateManager>(s => s.GetRequiredService<BlobStateManager>());
            }

            services
                .AddSingleton<IEventManager, EventManager>()
                .AddHostedService(s => (EventManager)s.GetRequiredService<IEventManager>());

            services.AddSingleton<IDynamicNuGetPackageLoader>(s =>
                new DynamicNuGetPackageLoader(NuGetPackageRepository.CreateForNuGetOrg(),
                    s.GetRequiredService<ILogger<DynamicNuGetPackageLoader>>()));

            services.AddHostedService<TimeMessagePublisher>();

            return services;
        }
    }
}
