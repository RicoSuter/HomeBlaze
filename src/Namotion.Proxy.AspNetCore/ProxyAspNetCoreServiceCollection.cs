using Namotion.Proxy;
using Namotion.Proxy.AspNetCore.Controllers;
using Namotion.Trackable.AspNetCore.Controllers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ProxyAspNetCoreServiceCollection
{
    /// <summary>
    /// Registers a generic controller with the signature 'ProxyController{TProxy} : ProxyControllerBase{TProxy} where TProxy : class'.
    /// </summary>
    /// <typeparam name="TController">The controller type.</typeparam>
    /// <typeparam name="TProxy">The proxy type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddProxyControllers<TProxy, TController>(this IServiceCollection services)
        where TController : ProxyControllerBase<TProxy>
        where TProxy : class, IProxy
    {
        services
            .AddControllers()
            .ConfigureApplicationPartManager(setup =>
            {
                setup.FeatureProviders.Add(new ProxyControllerFeatureProvider<TController>());
            });

        return services;
    }
}

