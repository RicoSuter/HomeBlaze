using Namotion.Trackable.AspNetCore.Controllers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableAspNetCoreServiceCollection
{
    /// <summary>
    /// Registers a generic controller with the signature 'VariablesController{TVariables} : TrackablesControllerBase{TVariables} where TVariables : class'.
    /// </summary>
    /// <typeparam name="TController">The controller type.</typeparam>
    /// <typeparam name="TTrackable">The trackable type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTrackableControllers<TTrackable, TController>(this IServiceCollection services)
        where TController : TrackablesControllerBase<TTrackable>
        where TTrackable : class
    {
        services
            .AddControllers()
            .ConfigureApplicationPartManager(setup =>
            {
                setup.FeatureProviders.Add(new TrackablesControllerFeatureProvider<TController>());
            });

        return services;
    }
}

