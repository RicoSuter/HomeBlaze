using Microsoft.Extensions.DependencyInjection.Extensions;
using Namotion.Trackable;
using Namotion.Trackable.Validation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableServiceCollection
{
    public static IServiceCollection AddTrackable<TTrackable>(this IServiceCollection services)
        where TTrackable : class
    {
        services
            .AddSingleton<TrackableContext<TTrackable>>()
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TTrackable>>().Object)
            .TryAddSingleton<ITrackableFactory, TrackableFactory>();

        return services;
    }

    public static IServiceCollection AddTrackableValidation(this IServiceCollection services)
    {
        services
            .TryAddSingleton<ITrackableInterceptor, ValidationTrackableInterceptor>()
;
        return services;
    }
}
