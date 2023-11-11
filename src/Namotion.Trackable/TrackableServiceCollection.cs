using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Namotion.Trackable;
using Namotion.Trackable.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableServiceCollection
{
    public static IServiceCollection AddTrackable<TTrackable>(this IServiceCollection services, 
        IEnumerable<ITrackableInterceptor>? trackableInterceptors = null, 
        IEnumerable<IInterceptor>? interceptors = null)
        where TTrackable : class
    {
        var trackableInterceptorsEnumerable = trackableInterceptors ?? Array.Empty<ITrackableInterceptor>();
        var interceptorsEnumerable = interceptors ?? Array.Empty<IInterceptor>();

        services
            .AddSingleton<TrackableContext<TTrackable>>()
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TTrackable>>().Object)
            .TryAddSingleton<ITrackableFactory>(sp => new TrackableFactory(sp,
                trackableInterceptorsEnumerable.Concat(sp.GetServices<ITrackableInterceptor>()),
                interceptorsEnumerable));

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
