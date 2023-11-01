using Castle.DynamicProxy;

using Namotion.Trackable;
using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class TrackableServiceCollection
{
    public static IServiceCollection AddTrackable<TTrackable>(this IServiceCollection services)
        where TTrackable : class
    {
        services
            .AddSingleton(sp =>
            {
                var propertyValidators = sp.GetServices<ITrackablePropertyValidator>();
                return new TrackableContext<TTrackable>(propertyValidators, sp);
            })
            .AddSingleton(sp => sp.GetRequiredService<TrackableContext<TTrackable>>().Object);

        return services;
    }
}

