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
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TTrackable>>().Object);

        return services;
    }

    public static IServiceCollection AddTrackableValidation(this IServiceCollection services)
    {
        services
            .AddSingleton<ITrackableInterceptor, ValidationTrackableInterceptor>()
;
        return services;
    }
}

